# Quasar CSS Helper

Style Vue components with Quasar helper classes first. Add local SCSS only when no helper can express the requirement.

## Internal

- Typography: `text-{h1|h2|h3|h4|h5|h6|subtitle1|subtitle2|body1|body2|caption|overline}`, `text-weight-{thin|light|regular|medium|bold|bolder}`. Text helpers: `text-{left|center|right|justify|bold|italic|no-wrap|strike|uppercase|lowercase|capitalize}`.
- Colors: brand `primary|secondary|accent|dark|positive|negative|info|warning`; palette `red|pink|purple|deep-purple|indigo|blue|light-blue|cyan|teal|green|light-green|lime|yellow|amber|orange|deep-orange|brown|grey|blue-grey`, with `-1` to `-14` shades. Use `text-{color}`, `bg-{color}`, or a component `color` prop.
- Spacing: `q-{p|m}{t|r|b|l|a|x|y}-{none|xs|sm|md|lg|xl}`. `p`/`m` mean padding/margin; directions are top/right/bottom/left/all/horizontal/vertical. `auto` is margin-only: `q-m{t|r|b|l|x|y}-auto`. Do not use breakpoint-aware spacing variants: this project does not enable `cssAddon`.
- Shadows: `no-shadow`, `inset-shadow`, `inset-shadow-down`, `shadow-{1..24}`, `shadow-up-{1..24}`, `shadow-transition`.
- Breakpoints: `xs` 0-599.98px, `sm` 600-1023.98px, `md` 1024-1439.98px, `lg` 1440-1919.98px, `xl` >=1920px. In SCSS, use variables such as `$breakpoint-xs-max` in media queries.
- Body classes: `body--{dark|light}`, `desktop|mobile`, `touch|no-touch`, `platform-{android|ios}`, `native-mobile|electron|bex|within-iframe`. `screen--{xs|sm|md|lg|xl}` exists only when enabled.
- Visibility and layers: `disabled` applies disabled cursor/opacity; `hidden` removes layout space; `invisible` keeps it; `transparent`; `dimmed|light-dimmed` add an overlay and cannot coexist with an existing `::after`; `ellipsis`, `ellipsis-2-lines`, `ellipsis-3-lines` truncate text (multi-line variants are WebKit-only); `z-top|z-max` raise stacking order.

## Custom

- Borders and cards: `border-radius-{4|6|8|12|16}` applies the local radius; `6` is currently 4px and `12` is 8px. `card-like` adds a white background, 4px radius, and subtle shadow; `card-like-borderless` keeps only the background and shadow. Prefer Quasar `rounded-borders`, `no-border`, `no-border-radius`, and `no-box-shadow` when they meet the need.
- Hover affordances: `hover-underline` makes text clickable and reveals a primary-colour underline on hover. `hover-card` makes an element clickable, then scales it to 1.03 and applies a stronger shadow. `clickable` adds 4px padding and a grey hover background. Use only for controls with a real click action.
- Scroll: `scroll-y` and `scroll-x` set one-axis scrolling; prefer Quasar `scroll`, `no-scroll`, `overflow-auto`, `overflow-hidden`, and `hide-scrollbar` for their documented cross-platform behavior. `scroll_pretty` supplies a 6px WebKit scrollbar. `hover-scroll` hides its nonstandard `overflow: overlay` scrollbar until hover, so restrict it to Chromium/Electron surfaces.
- Size: `half-height` and `half-width` set 50%; `height-0` supports flex layouts; `height-{200|300|400|500|600}` and `width-{200|300|400|500|600}` set fixed pixels. `max-width-{200|300|400}` sets the corresponding maximum width. Do not use `max-height-*`: despite the name, each currently sets `max-width`. `width-700` and `width-800` currently also set 600px.
- Text and background: `ellipsis-left` truncates from the beginning; wrap its content in `<span dir="ltr">` to keep a leading number and space in order. `ellipsis` is the local single-line truncation rule and overlaps Quasar's helper. `bg-transparent` sets a transparent background.
- Content editing: a `[contentEditable='true']:empty` element with a `placeholder` attribute shows its attribute as muted placeholder text.
- Responsive columns: `col-auto-2` is full width below 576px and half width at 576px or larger; it also sets `width: 0` for ECharts resizing. `col-auto-4` is 1/2/3/4 columns at 0/576/768/992px, with 4px padding. These breakpoints are local, not Quasar screen breakpoints.
- Positioning: use Quasar `fullscreen`, `fixed`, `absolute`, `relative-position`, and their `{fixed|absolute}-{top|right|bottom|left|full|center}` variants for placement. `absolute-center` requires a `relative-position` container. Prefer `row` and `justify-*` over `float-{left|right}`; use `on-left|on-right` for the small icon-to-sibling gap and `vertical-{top|middle|bottom}` for inline vertical alignment.
- Other Quasar helpers: use `non-selectable`, `no-pointer-events|all-pointer-events`, and `cursor-{pointer|not-allowed|inherit|none}` for interaction state; `fit|full-height|full-width|window-height|window-width|block` for size; `rotate-{45|90|135|180|225|270|315}` and `flip-{horizontal|vertical}` for orientation.
