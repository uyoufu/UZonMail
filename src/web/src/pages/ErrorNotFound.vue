<template>
  <div class="fullscreen bg-grey-12 text-primary text-center q-pa-md flex flex-center">
    <div>
      <q-icon :name="resolveSvgFullName('undraw_page_not_found')" size="75vh"></q-icon>

      <div class="text-h6" style="opacity:.5">
        Oops. Nothing here...
      </div>

      <q-btn class="q-mt-xl" color="white" text-color="secondary" unelevated to="/" label="Go Home" no-caps />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { resolveSvgFullName } from 'src/utils/svgHelper'
</script>
