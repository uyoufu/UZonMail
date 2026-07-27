<template>
  <q-card class="column items-center justify-center q-pa-md text-subtitle1">
    <div class="row items-center">
      <UserAvatar size="50px" />
      <h6 class="q-ml-md text-secondary">{{ userInfo.userId }}</h6>
    </div>

    <div v-if="createDate" class="row justify-start items-center">
      <span>{{ t('profile.registrationDate') }}</span>
      <span class="text-secondary">{{ createDate }}</span>
    </div>

    <div v-if="userRole" class="row justify-start items-center">
      <span>{{ t('profile.accountRole') }}</span>
      <span class="text-secondary">{{ userRole }}</span>
    </div>

    <div class="row justify-end items-center q-mt-lg">
      <CommonBtn :label="t('profile.changeAvatar')" color="secondary" class="q-mr-md" @click="onChangeUserAvatar" />
      <CommonBtn :label="t('profile.changePassword')" @click="onChangeUserPassword" />
    </div>
  </q-card>
</template>

<script lang="ts" setup>
import UserAvatar from 'src/components/userAvatar/UserAvatar.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'
import dayjs from 'dayjs'

import { useUserInfoStore } from 'src/stores/user'
import { t } from 'src/i18n/helpers'
const userInfoStore = useUserInfoStore()

import { getUserInfo, changeUserPassword, updateUserAvatar } from 'src/api/user'
// 获取用户的信息
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const userInfo: Ref<Record<string, any>> = ref({})
onMounted(async () => {
  const { data } = await getUserInfo(userInfoStore.userId)
  userInfo.value = data
})

// 创建日期
const createDate = computed(() => {
  if (!userInfo.value.createDate) return ''
  return dayjs(userInfo.value.createDate).format('YYYY-MM-DD')
})

// 当前角色
const userRole = computed(() => {
  if (userInfo.value.isSuperAdmin) return t('profile.superAdmin')
  return t('profile.normalUser')
})

/**
 * 修改密码
 */
import { showDialog, showComponentDialog } from 'src/components/lowCode/PopupDialog'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { notifySuccess } from 'src/utils/dialog'
async function onChangeUserPassword () {
  const result = await showDialog({
    title: t('profile.changePassword'),
    fields: [
      {
        name: 'oldPassword',
        label: t('profile.oldPassword'),
        type: LowCodeFieldType.text,
        placeholder: t('profile.enterOldPassword'),
        value: ''
      },
      {
        name: 'newPassword',
        label: t('profile.newPassword'),
        type: LowCodeFieldType.password,
        placeholder: t('profile.enterNewPassword'),
        value: ''
      }
    ],
    onOkMain: async (modelValue) => {
      const { data } = await changeUserPassword(modelValue.oldPassword, modelValue.newPassword)
      return data
    }
  })

  if (!result.ok) return

  notifySuccess(t('profile.passwordChanged', { password: result.data.newPassword }))
}

/**
 * 修改头像
 */
import ImageCropper from 'src/components/imageCropper/ImageCropper.vue'
import { openFileSelector, bufferToBase64Png } from 'src/utils/file'
async function onChangeUserAvatar () {
  // 选择文件
  const buffer = await openFileSelector()
  if (!buffer) return

  const blobResult = await showComponentDialog(ImageCropper, {
    img: bufferToBase64Png(buffer as ArrayBuffer)
  })
  if (!blobResult.ok) return

  // 上传 blob 到服务器
  const { data } = blobResult
  const { data: avatarUrl } = await updateUserAvatar(data as Blob)
  // 将头像数据更新到 store 中
  userInfoStore.updateUserAvatar(avatarUrl)
}
</script>

<style lang="scss" scoped></style>
