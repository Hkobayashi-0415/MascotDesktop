# accounts/models.py
from django.db import models
from django.contrib.auth.models import AbstractBaseUser, BaseUserManager, PermissionsMixin
from django.contrib.auth.models import Group, Permission # Group, Permission も忘れずにインポート
from django.conf import settings # UserCharacterAffinity で参照する場合
from character_management.models import Characters


# ユーザーマネージャーを定義
class CustomUserManager(BaseUserManager):
    def create_user(self, name, password=None, **extra_fields):
        if not name:
            raise ValueError('ユーザー名は必須です')
        user = self.model(name=name, **extra_fields)
        user.set_password(password) # AbstractBaseUser が提供する set_password メソッドでハッシュ化
        user.save(using=self._db)
        return user

    def create_superuser(self, name, password=None, **extra_fields):
        extra_fields.setdefault('is_staff', True)
        extra_fields.setdefault('is_superuser', True)
        extra_fields.setdefault('is_active', True)

        if extra_fields.get('is_staff') is not True:
            raise ValueError('スーパーユーザーは is_staff=True である必要があります。')
        if extra_fields.get('is_superuser') is not True:
            raise ValueError('スーパーユーザーは is_superuser=True である必要があります。')

        # create_user を呼び出してユーザーを作成
        return self.create_user(name, password, **extra_fields)


# カスタムユーザーモデルを定義
class Users(AbstractBaseUser, PermissionsMixin):
    ''' ユーザーテーブル：アプリケーションを利用するユーザーの基本情報と認証情報を管理する。'''
    # AbstractBaseUser が password と last_login フィールドを内部的に提供します。
    # PermissionsMixin が is_superuser, groups, user_permissions フィールドを内部的に提供します。

    # ユーザー名フィールド (USERNAME_FIELD として指定)
    name = models.CharField(
        max_length=50,
        unique=True, # ユーザー名はユニークである必要があります
        verbose_name='ユーザー名'
    )

    # カスタムで追加するフィールド (例: default_character_id)
    # このフィールドは他のモデルを参照するため、参照先のモデル定義後にマイグレーションが必要です。
    # Character モデルが character_management アプリにあるため、ForeignKey で参照します。
    # from character_management.models import Characters (UserCharacterAffinity と一緒にインポートするか、ここにもインポート)
    # default_character = models.ForeignKey(Characters, on_delete=models.SET_NULL, null=True, blank=True, verbose_name='デフォルトキャラクター')
    # または、IntegerField のまま一時的に持つ場合
    default_character_id = models.IntegerField(
        default=1, # デフォルト値
        verbose_name='デフォルトキャラクターID'
    )

    # is_staff フィールド (PermissionsMixin で追加されるものを利用しますが、明示的に定義することが多い)
    # BooleanField にする必要があります
    is_staff = models.BooleanField(
        default=False,
        verbose_name='スタッフステータス',
        help_text='このユーザーがAdminサイトにログインできるか指定します。'
    )

    # is_active フィールド (PermissionsMixin で追加されるものを利用しますが、明示的に定義することが多い)
    # BooleanField にする必要があります
    is_active = models.BooleanField(
        default=True,
        verbose_name='アクティブ',
        help_text='このユーザーがアクティブとして扱われるか指定します。非選択にするとユーザーを無効にします'
    )

    # 作成日時と更新日時
    created_at = models.DateTimeField(auto_now_add=True, verbose_name='作成日時')
    updated_at = models.DateTimeField(auto_now=True, verbose_name='更新日時')

    # PermissionsMixin が提供する groups フィールドを明示的に定義し related_name を指定
    # ★★★★ これも自分で定義する必要があります ★★★★
    groups = models.ManyToManyField(
        Group,
        verbose_name='groups',
        blank=True,
        help_text='The groups this user belongs to. A user will get all permissions '
                  'granted to each of their groups.',
        related_name="accounts_users_set", # <-- ここにユニークな名前を指定
        related_query_name="account_user",
    )

    # PermissionsMixin が提供する user_permissions フィールドを明示的に定義し related_name を指定
    # ★★★★ これも自分で定義する必要があります ★★★★
    user_permissions = models.ManyToManyField(
        Permission,
        verbose_name='user permissions',
        blank=True,
        help_text='Specific permissions for this user.',
        related_name="accounts_user_permissions", # <-- ここにも別のユニークな名前を指定
        related_query_name="account_user",
    )
    # ★★★★ 自分で定義する必要があるフィールドはここまで ★★★★


    # マネージャーを設定
    objects = CustomUserManager()

    # ユーザー認証に使用する一意の識別子フィールドを指定 (必須)
    USERNAME_FIELD = 'name' # <-- この 'name' に対応するフィールドが上で定義されている必要があります。

    # createsuperuser コマンド実行時に USERNAME_FIELD および パスワード以外で必須入力とするフィールドを指定 (任意)
    REQUIRED_FIELDS = [] # 例: ['email']


    class Meta:
        verbose_name = 'ユーザー'
        verbose_name_plural = 'ユーザー一覧'
        ordering = ['name'] # <-- ここでも 'name' フィールドを参照しています。
        # ... (他の Meta 設定) ...

    def __str__(self):
        return self.name # <-- ここでも 'name' フィールドを使っています。




class UserCharacterAffinity(models.Model):
    ''' ユーザー・キャラクター関連テーブル：ユーザーとキャラクター間の関連情報（デフォルトキャラクター設定など）を管理する。'''
    # user フィールドは設定済み AUTH_USER_MODEL (accounts.Users) を自動的に参照します
    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, verbose_name='ユーザー')
    character = models.ForeignKey(Characters, on_delete=models.CASCADE, verbose_name='キャラクター')
    affinity_value = models.IntegerField(default=0, verbose_name='親密度')
    is_default_character = models.BooleanField(default=False, verbose_name='デフォルトキャラクターフラグ')
    selected_audio_voice_id = models.IntegerField(null=True, blank=True,  verbose_name='選択音声ID')

    class Meta:
        unique_together = ('user', 'character')
        verbose_name = 'ユーザーとキャラクターの関連'
        verbose_name_plural = 'ユーザーとキャラクターの関連一覧'
        ordering = ['user', 'character']

    def __str__(self):
        return f'{self.user.name} - {self.character.name} (Affinity: {self.affinity_value})'