
<script>
import { notifyError } from '../components/iPrompt'
// 用于强制项目登陆及跳转用
export default {
  beforeCreate() {
    if (!this.$store.getters.loginedProjectGroupId) {
      // 直接跳转到项目登陆界面
      notifyError('请先登陆项目')
      this.$router.replace({
        path: '/projects/login'
      })
    }
  }
}
</script>
