from django.db import models

class Characters(models.Model):
    ''' キャラクターテーブル：アプリケーションに登場するキャラクターの基本情報を管理する。'''
    name = models.CharField(max_length=50, unique=True, verbose_name='キャラクター名')
    description = models.TextField(verbose_name='キャラクター説明')
    default_image_category = models.CharField(max_length=50, default='default', verbose_name='画像カテゴリ')
    default_audio_voice_id = models.IntegerField(null=True, blank=True, verbose_name='デフォルト音声ID')
    default_audio_on = models.BooleanField(default=False, verbose_name='デフォルト音声ON/OFF')
    created_at = models.DateTimeField(auto_now_add=True, verbose_name='作成日時')
    updated_at = models.DateTimeField(auto_now=True, verbose_name='更新日時')

    class Meta:
        verbose_name = 'キャラクター'
        verbose_name_plural = 'キャラクター一覧'
        ordering = ['id']

    def __str__(self):
        return self.name
    
class Image(models.Model):
    ''' 画像テーブル：キャラクターの画像ファイル情報と表示条件を管理する。'''
    character = models.ForeignKey(Characters, on_delete=models.CASCADE, verbose_name='キャラクター')
    affinity_level = models.IntegerField(null=True, blank=True,  verbose_name='親密度レベル')
    category = models.CharField(max_length=50, default='default', verbose_name='画像カテゴリ', \
        help_text='例: default, happy, sad など。Charactersモデルのdefault_image_categoryと対応します。')
    file_name = models.CharField(max_length=255, null=True, verbose_name='画像ファイル名/パス')
    trigger_condition = models.CharField(max_length=255, null=True, blank=True, verbose_name='表示トリガー条件')

    class Meta:
        verbose_name = '画像'
        verbose_name_plural = '画像一覧'
        ordering = ['character', 'category', 'file_name']
        unique_together = ('character', 'category', 'file_name')

    def __str__(self):
        return f'{self.character.name} - {self.category} ({self.file_name})'
