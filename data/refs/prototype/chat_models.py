from django.db import models
from character_management.models import Characters, Image
from accounts.models import Users

class Conversations(models.Model):
    ''' conversations：ユーザーとキャラクター間の会話の履歴を管理する。'''
    user = models.ForeignKey(Users, on_delete=models.CASCADE, verbose_name='ユーザー')
    character = models.ForeignKey(Characters, on_delete=models.CASCADE, verbose_name='キャラクター')
    role = models.CharField(max_length=10, verbose_name='発言者')
    content = models.TextField(verbose_name='発言内容')
    timestamp = models.DateTimeField(auto_now_add=True, verbose_name='発言日時')
    sentiment_score = models.FloatField(null=True, blank=True, verbose_name='感情分析スコア')
    selected_image = models.ForeignKey(Image, on_delete=models.SET_NULL, null=True, \
        blank=True, verbose_name='表示された画像')
    affinity_change_applied = models.IntegerField(null=True, blank=True, verbose_name='適用された親密度変動')
    affinity_rules_triggered = models.TextField(null=True, blank=True, verbose_name='トリガーされた親密度ルール')
    affinity_value_after = models.IntegerField(null=True, blank=True, verbose_name='ターン後の親密度')

    class Meta:
        verbose_name = '会話履歴'
        verbose_name_plural = '会話履歴一覧'
        ordering = ['-timestamp']

    def __str__(self):
        return f'{self.timestamp:%Y-%m-%d %H:%M} - {self.user.name}: {self.content[:30]}...'