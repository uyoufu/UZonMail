import { CrawlerStatus, getCrawlerTaskCountInfos } from 'src/api/pro/crawlerTask'

/* eslint-disable @typescript-eslint/no-explicit-any */
export function useCountUpdator (rowsRef: Ref<Record<string, any>[]>) {
  const runningRows = computed(() => rowsRef.value.filter(x => x.status === CrawlerStatus.running))
  const interval = 5000

  // 进行轮询更新
  // eslint-disable-next-line @typescript-eslint/no-misused-promises
  const intervelId = setInterval(async () => {
    if (runningRows.value.length > 0) return

    const { data: countInfos } = await getCrawlerTaskCountInfos(runningRows.value.map(x => x.id))
    for (const countInfo of countInfos) {
      const row = rowsRef.value.find(x => x.id === countInfo.id)
      if (row) { row.count = countInfo.count }
    }
  }, interval)

  onUnmounted(() => {
    clearInterval(intervelId)
  })
}
