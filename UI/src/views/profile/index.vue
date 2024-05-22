<template>
  <div class="text-center">
    <div class="column justify-center">
      <div class="card-img avatar" @click="triger = true">
        <img :src="avatar" class="card-img avatar">
        <q-tooltip anchor="center middle" self="center middle">
          {{ $t('avatarTooltip') }}
        </q-tooltip>
      </div>
      <div>{{ userName }}</div>
    </div>

    <avatar-cropper
      v-model="triger"
      :upload-url="uploadUrl"
      :request-options="requestOptions"
      @uploading="handleUploading"
      @uploaded="handleUploaded"
      @completed="handleCompleted"
      @error="handlerError"
    />
  </div>
</template>

<script>
import AvatarCropper from 'vue-avatar-cropper'
import { notifyError, notifySuccess } from '@/components/iPrompt'
import { updateUserAvatar } from '@/api/user'

export default {
  components: { AvatarCropper },
  data() {
    return {
      triger: false,
      uploadUrl: process.env.VUE_APP_BASE_API + '/file',
      requestOptions: {
        method: 'post',
        headers: {
          'X-Token': this.$store.getters.token
        }
      }
    }
  },
  computed: {
    userName() {
      return this.$store.getters.name
    },
    avatar() {
      return this.$store.getters.avatar
    }
  },
  methods: {
    handleUploading(form, xhr) {
      this.message = 'uploading...'
    },
    async handleUploaded({ response }) {
      if (response.ok) {
        // Maybe you need call vuex action to
        // update user avatar, for example:
        // 获取 avatar 完整路径
        // const avatar = `${process.env.VUE_APP_BASE_API}/files/`
        // 获取 url
        const rs = await response.json()
        console.log('handleUploaded:', rs, this.$store.getters.name)

        if (rs.data.length < 1) {
          notifyError(this.$t('uploadedAvatarError'))
          return
        }

        let avatar = `${process.env.VUE_APP_BASE_API}/file?fileId=${rs.data[0]}`

        // 进行编码
        avatar = encodeURI(avatar)

        this.$store.dispatch('user/setAvatar', avatar)

        console.log('handleUploaded avatar:', avatar)

        // 更新数组库中的数组
        await updateUserAvatar(avatar, this.$store.getters.name)
      }
    },

    handleCompleted(response, form, xhr) {
      console.log('handleCompleted:', response)
      notifySuccess(this.$t('successModified'))
    },

    handlerError(message, type, xhr) {
      notifyError(this.$t('uploadedAvatarError') + message)
    }
  }
}
</script>

<style>
.avatar {
  width: 160px;
  border-radius: 6px;
  display: block;
  margin: 20px auto;
}
</style>
