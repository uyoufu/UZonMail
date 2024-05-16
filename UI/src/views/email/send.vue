<template>
  <EmailGroup class="email-group" :apis="sendApis" />
</template>

<script>
import EmailGroup from './components/emailGroup.vue'

export default {
  components: { EmailGroup },
  data() {
    return {
      sendApis: {}
    }
  }
}
</script>

<style lang="scss">
</style>
