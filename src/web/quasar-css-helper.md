# Quasar CSS Helper

Style Vue components with Quasar helper classes first. Add local SCSS only when no helper can express the requirement.

- Typography: `text-{h1|h2|h3|h4|h5|h6|subtitle1|subtitle2|body1|body2|caption|overline}`, `text-weight-{thin|light|regular|medium|bold|bolder}`. Text helpers: `text-{left|center|right|justify|bold|italic|no-wrap|strike|uppercase|lowercase|capitalize}`.
- Colors: brand `primary|secondary|accent|dark|positive|negative|info|warning`; palette `red|pink|purple|deep-purple|indigo|blue|light-blue|cyan|teal|green|light-green|lime|yellow|amber|orange|deep-orange|brown|grey|blue-grey`, with `-1` to `-14` shades. Use `text-{color}`, `bg-{color}`, or a component `color` prop.
- Spacing: `q-{p|m}{t|r|b|l|a|x|y}-{none|xs|sm|md|lg|xl}`. `p`/`m` mean padding/margin; directions are top/right/bottom/left/all/horizontal/vertical. `auto` is margin-only: `q-m{t|r|b|l|x|y}-auto`. Do not use breakpoint-aware spacing variants: this project does not enable `cssAddon`.
- Shadows: `no-shadow`, `inset-shadow`, `inset-shadow-down`, `shadow-{1..24}`, `shadow-up-{1..24}`, `shadow-transition`.
- Breakpoints: `xs` 0-599.98px, `sm` 600-1023.98px, `md` 1024-1439.98px, `lg` 1440-1919.98px, `xl` >=1920px. In SCSS, use variables such as `$breakpoint-xs-max` in media queries.
- Body classes: `body--{dark|light}`, `desktop|mobile`, `touch|no-touch`, `platform-{android|ios}`, `native-mobile|electron|bex|within-iframe`. `screen--{xs|sm|md|lg|xl}` exists only when enabled.
- Visibility and layers: `disabled` applies disabled cursor/opacity; `hidden` removes layout space; `invisible` keeps it; `transparent`; `dimmed|light-dimmed` add an overlay and cannot coexist with an existing `::after`; `ellipsis`, `ellipsis-2-lines`, `ellipsis-3-lines` truncate text (multi-line variants are WebKit-only); `z-top|z-max` raise stacking order.
