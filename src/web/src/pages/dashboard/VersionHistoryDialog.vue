<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide">
    <q-card class="version-history-dialog">
      <q-card-section class="row items-center no-wrap q-py-sm">
        <div class="text-subtitle1">{{ translateDashboardPage('versionHistoryTitle') }}</div>
        <q-space />
        <CommonBtn flat dense icon="close" color="grey-7" :tooltip="translateDashboardPage('versionHistoryClose')"
          @click="onDialogCancel" />
      </q-card-section>

      <q-separator />

      <q-card-section class="q-pa-none">
        <div class="version-history-dialog__content">
          <q-list separator class="hover-scroll" style="min-width: 200px;">
            <q-item v-for="(release, index) in releases" :key="release.version" clickable
              :active="selectedVersion === release.version" @click="onReleaseClick(release.version)">
              <q-item-section>
                <q-item-label>{{ release.version }}</q-item-label>
                <q-item-label v-if="release.publishedAt" caption>{{ release.publishedAt }}</q-item-label>
              </q-item-section>
              <q-item-section v-if="isDesktopClient && index === 0" side>
                <CommonBtn flat dense icon="update" color="secondary" size="md"
                  :label="translateDashboardPage('newVersionUpdate')"
                  :tooltip="translateDashboardPage('newVersionUpdate')" :disable="isStartingDesktopUpdate"
                  @click.stop="onLatestVersionUpdateClick" />
              </q-item-section>
            </q-item>
          </q-list>

          <section class="version-history-dialog__details q-pa-md hover-scroll">
            <div v-if="isLoading" class="row items-center justify-center full-height text-grey-7">
              <q-spinner-dots class="q-mr-sm" size="24px" />
              {{ translateDashboardPage('versionHistoryLoading') }}
            </div>
            <div v-else-if="hasLoadFailed" class="row items-center justify-center full-height text-negative">
              {{ translateDashboardPage('versionHistoryLoadFailed') }}
            </div>
            <div v-else-if="!selectedRelease" class="row items-center justify-center full-height text-grey-7">
              {{ translateDashboardPage('versionHistoryEmpty') }}
            </div>
            <template v-else>
              <div class="text-subtitle1">{{ selectedRelease.version }}</div>
              <div v-if="selectedRelease.publishedAt" class="text-caption text-grey-7 q-mt-xs">
                {{ translateDashboardPage('versionHistoryPublishedAt', { date: selectedRelease.publishedAt }) }}
              </div>
              <div v-for="(section, index) in selectedRelease.sections" :key="`${section.title ?? 'general'}-${index}`"
                class="q-mt-md">
                <div v-if="section.title" class="text-subtitle2 q-mb-xs">{{ section.title }}</div>
                <ul class="q-my-none q-pl-md">
                  <li v-for="(entry, entryIndex) in section.entries" :key="entryIndex" class="q-mb-xs">
                    <template v-for="(fragment, fragmentIndex) in entry.fragments" :key="fragmentIndex">
                      <a v-if="fragment.type === VersionHistoryFragmentType.Link && fragment.fileName" :href="fragment.url"
                        :download="fragment.fileName" class="text-primary cursor-pointer q-mr-xs"
                        @click.prevent="onVersionFileDownloadClick(fragment)">
                        {{ fragment.text }}
                      </a>
                      <a v-else-if="fragment.type === VersionHistoryFragmentType.Link" :href="fragment.url" target="_blank"
                        rel="noopener noreferrer" class="text-primary q-mr-xs">
                        {{ fragment.text }}
                      </a>
                      <span v-else>{{ fragment.text }}</span>
                    </template>
                  </li>
                </ul>
              </div>
            </template>
          </section>
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import { useDialogPluginComponent } from 'quasar'
import { translateDashboardPage } from 'src/i18n/helpers'
import { notifyError } from 'src/utils/dialog'
import { saveFileSmart } from 'src/utils/file'
import { startDesktopUpdate } from './desktopUpdate'
import {
  getVersionHistory,
  VersionHistoryFragmentType,
  type IVersionHistoryEntryFragment,
  type IVersionHistoryRelease
} from './versionHistory'

const props = defineProps<{
  locale: string
  isDesktopClient: boolean
}>()

defineEmits([...useDialogPluginComponent.emits])
const { dialogRef, onDialogCancel, onDialogHide } = useDialogPluginComponent()

const releases = ref<IVersionHistoryRelease[]>([])
const selectedVersion = ref('')
const isLoading = ref(true)
const hasLoadFailed = ref(false)
const isStartingDesktopUpdate = ref(false)
const selectedRelease = computed(() => releases.value.find(release => release.version === selectedVersion.value))

async function onLoadVersionHistory(): Promise<void> {
  try {
    releases.value = await getVersionHistory(props.locale)
    selectedVersion.value = releases.value[0]?.version ?? ''
  } catch {
    hasLoadFailed.value = true
  } finally {
    isLoading.value = false
  }
}

function onReleaseClick(version: string): void {
  selectedVersion.value = version
}

async function onVersionFileDownloadClick(fragment: IVersionHistoryEntryFragment): Promise<void> {
  if (fragment.type !== VersionHistoryFragmentType.Link || !fragment.fileName || !fragment.url) return
  await saveFileSmart(fragment.fileName, fragment.url)
}

async function onLatestVersionUpdateClick(): Promise<void> {
  if (isStartingDesktopUpdate.value) return

  isStartingDesktopUpdate.value = true
  try {
    if (!await startDesktopUpdate()) {
      notifyError(translateDashboardPage('newVersionUpdateStartFailed'))
    }
  } catch {
    notifyError(translateDashboardPage('newVersionUpdateStartFailed'))
  } finally {
    isStartingDesktopUpdate.value = false
  }
}

onMounted(() => {
  void onLoadVersionHistory()
})
</script>

<style lang="scss" scoped>
.version-history-dialog {
  width: 900px;
  max-width: 90vw;
}

.version-history-dialog__content {
  display: flex;
  height: min(70vh, 560px);
}

.version-history-dialog__versions {
  flex: 0 0 240px;
  overflow-y: auto;
  border-right: 1px solid var(--q-separator-color);
}

.version-history-dialog__details {
  flex: 1;
  min-width: 0;
  overflow-y: auto;
}

@media (max-width: 599px) {
  .version-history-dialog {
    max-width: 96vw;
  }

  .version-history-dialog__content {
    flex-direction: column;
    height: 72vh;
  }

  .version-history-dialog__versions {
    flex-basis: 36%;
    border-right: 0;
    border-bottom: 1px solid var(--q-separator-color);
  }
}
</style>
