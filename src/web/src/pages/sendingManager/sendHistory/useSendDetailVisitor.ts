import { useRouter } from "vue-router"

export function useSendDetailVisitor () {
  const router = useRouter()

  /**
   * 访问发件明细页面
   * @param sendingGroupId
   * @param tagName
   */
  async function visitSendDetailTable (sendingGroupId: number, tagName: string) {
    await router.push({
      name: 'SendDetailTable',
      query: {
        sendingGroupId,
        tagName
      }
    })
  }

  return { visitSendDetailTable }
}
