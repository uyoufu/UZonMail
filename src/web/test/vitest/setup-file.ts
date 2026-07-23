import { config } from '@vue/test-utils';
import { Notify, Quasar } from 'quasar';

config.global.plugins = [[Quasar, { plugins: { Notify } }]];
