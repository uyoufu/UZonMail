<template>
  <q-card class="column items-center justify-center q-pa-md text-subtitle1">
    <div class="row items-center">
      <UserAvatar size="50px" />
      <h6 class="q-ml-md text-secondary">{{ userInfo.userId }}</h6>
    </div>

    <div v-if="createDate" class="row justify-start items-center">
      <span>注册日期：</span>
      <span class="text-secondary">{{ createDate }}</span>
    </div>

    <div v-if="userRole" class="row justify-start items-center">
      <span>账户角色：</span>
      <span class="text-secondary">{{ userRole }}</span>
    </div>

    <div class="row justify-end items-center q-mt-lg">
      <CommonBtn label="修改头像" color="secondary" class="q-mr-md" @click="onChangeUserAvatar" />
      <CommonBtn label="修改密码" @click="onChangeUserPassword" />
    </div>
  </q-card>
</template>

<script lang="ts" setup>
import UserAvatar from 'src/components/userAvatar/UserAvatar.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'
import dayjs from 'dayjs'

import { useUserInfoStore } from 'src/stores/user'
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
  if (userInfo.value.isSuperAdmin) return '超级管理员'
  return '普通用户'
})

/**
 * 修改密码
 */
import { showDialog, showComponentDialog } from 'src/components/popupDialog/PopupDialog'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { notifySuccess } from 'src/utils/dialog'
async function onChangeUserPassword () {
  const result = await showDialog({
    title: '修改密码',
    fields: [
      {
        name: 'oldPassword',
        label: '旧密码',
        type: PopupDialogFieldType.text,
        placeholder: '请输入旧密码',
        value: ''
      },
      {
        name: 'newPassword',
        label: '新密码',
        type: PopupDialogFieldType.password,
        placeholder: '请输入新密码',
        value: ''
      }
    ],
    onOkMain: async (modelValue) => {
      const { data } = await changeUserPassword(modelValue.oldPassword, modelValue.newPassword)
      return data
    }
  })

  if (!result.ok) return

  notifySuccess(`密码修改成功! 新密码为：${result.data.newPassword}`)
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
