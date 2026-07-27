<template>
  <div class="card-like q-pa-md">
    <div v-if="htmlContent" v-html="htmlContent"></div>
    <div v-else>
      <div class="column justify-center items-center">
        <h6 class="text-primary">{{ t('pages.sponsor.title') }}</h6>
        <p class="q-mx-md">{{ t('pages.sponsor.introduction') }}</p>
        <p class="q-mx-md">{{ t('pages.sponsor.supportMotivation') }}</p>
        <p class="q-mx-md">{{ t('pages.sponsor.thanks') }}</p>
        <div class="row justify-center items-center q-gutter-lg">
          <div class="column items-center">
            <img class="max-width-200 hover-card" src="https://s2.loli.net/2024/06/03/snNj5Q9wfuV8cRH.png" :alt="t('pages.sponsor.alipayCoupon')">
            <div class="q-mt-sm">{{ t('pages.sponsor.scanAlipayCoupon') }}</div>
          </div>
          <div class="column items-center">
            <img class="max-width-200 hover-card" src="https://s2.loli.net/2024/06/03/yJ1HEAvRScIoMwh.png" :alt="t('pages.sponsor.alipay')">
            <div class="q-mt-sm">{{ t('pages.sponsor.alipay') }}</div>
          </div>
          <div class="column items-center">
            <img class="max-width-200 hover-card" src="https://s2.loli.net/2024/06/03/3CQtphrsW1DF6bd.png" :alt="t('pages.sponsor.wechat')">
            <div class="q-mt-sm">{{ t('pages.sponsor.wechat') }}</div>
          </div>
        </div>
        <div class="q-mb-xl"></div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { getSponsorPageContent } from 'src/api/sponsor'
import { t } from 'src/i18n/helpers'

const htmlContent = ref('')
onMounted(async () => {
  // 从 gitee 上获取数据
  const { data } = await getSponsorPageContent()
  if (data) htmlContent.value = data
})
</script>

<style lang="scss" scoped>
.sponsor-iframe {
  width: 100%;
  height: 100%;
  border: none;
}
</style>
