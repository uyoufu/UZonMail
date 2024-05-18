<template>
  <div class="navbar">
    <hamburger :is-active="sidebar.opened" class="hamburger-container" @toggleClick="toggleSideBar" />

    <breadcrumb class="breadcrumb-container" />

    <div class="right-menu">
      <Lang />
      <el-dropdown class="avatar-container" trigger="click">
        <div class="avatar-wrapper">
          <img :src="avatar" class="user-avatar">
          <i class="el-icon-caret-bottom" />
        </div>
        <el-dropdown-menu slot="dropdown">
          <router-link to="/">
            <el-dropdown-item>{{ $t('home') }}</el-dropdown-item>
          </router-link>
          <a target="_blank" href="https://galens.uamazing.cn/posts/2020/2QMK677.html">
            <el-dropdown-item>{{ $t('docs') }}</el-dropdown-item>
          </a>

          <router-link to="/profile/index">
            <el-dropdown-item>{{ $t('profile') }}</el-dropdown-item>
          </router-link>

          <el-dropdown-item divided @click.native="logout">
            <span style="display: block">{{ $t('logout') }}</span>
          </el-dropdown-item>
        </el-dropdown-menu>
      </el-dropdown>

      <!--添加全局消息-->
      <q-dialog v-model="isShowGlobalMessage" persistent>
        <GlobalMesssage :message="websocketMsg" @close="isShowGlobalMessage = false" />
      </q-dialog>
    </div>
  </div>
</template>

<script>
import { mapGetters } from 'vuex'
import Breadcrumb from '@/components/Breadcrumb'
import Hamburger from '@/components/Hamburger'

import MixMessage from '../mixin/mix-message'
import GlobalMesssage from './GlobalMessage.vue'

export default {
  components: {
    Breadcrumb,
    Hamburger,
    GlobalMesssage
  },

  mixins: [MixMessage],

  computed: {
    ...mapGetters(['sidebar', 'avatar'])

    // avatarx(){
    //   return "avatar + 'imageView2/1/w/80/h/80'"
    // }
  },
  methods: {
    toggleSideBar() {
      this.$store.dispatch('app/toggleSideBar')
    },
    async logout() {
      await this.$store.dispatch('user/logout')
      this.$router.push(`/login?redirect=${this.$route.fullPath}`)
    }
  }
}
</script>

<style lang="scss" scoped>
.navbar {
  height: 50px;
  overflow: hidden;
  position: relative;
  background: #fff;
  box-shadow: 0 1px 4px rgba(0, 21, 41, 0.08);

  .hamburger-container {
    line-height: 46px;
    height: 100%;
    float: left;
    cursor: pointer;
    transition: background 0.3s;
    -webkit-tap-highlight-color: transparent;

    &:hover {
      background: rgba(0, 0, 0, 0.025);
    }
  }

  .breadcrumb-container {
    float: left;
  }

  .right-menu {
    float: right;
    height: 100%;
    line-height: 24px;

    &:focus {
      outline: none;
    }

    .right-menu-item {
      display: inline-block;
      padding: 0 8px;
      height: 100%;
      font-size: 18px;
      color: #5a5e66;
      vertical-align: text-bottom;

      &.hover-effect {
        cursor: pointer;
        transition: background 0.3s;

        &:hover {
          background: rgba(0, 0, 0, 0.025);
        }
      }
    }

    .avatar-container {
      margin-right: 30px;

      .avatar-wrapper {
        margin-top: 5px;
        position: relative;

        .user-avatar {
          cursor: pointer;
          width: 40px;
          height: 40px;
          border-radius: 10px;
        }

        .el-icon-caret-bottom {
          cursor: pointer;
          position: absolute;
          right: -20px;
          top: 25px;
          font-size: 12px;
        }
      }
    }
  }
}
</style>
