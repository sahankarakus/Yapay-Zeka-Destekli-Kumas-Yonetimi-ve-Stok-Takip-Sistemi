---
name: Industrial Clarity
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#434655'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#737686'
  outline-variant: '#c3c6d7'
  surface-tint: '#0053db'
  primary: '#004ac6'
  on-primary: '#ffffff'
  primary-container: '#2563eb'
  on-primary-container: '#eeefff'
  inverse-primary: '#b4c5ff'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  on-secondary-container: '#54647a'
  tertiary: '#4d556b'
  on-tertiary: '#ffffff'
  tertiary-container: '#656d84'
  on-tertiary-container: '#eef0ff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dbe1ff'
  primary-fixed-dim: '#b4c5ff'
  on-primary-fixed: '#00174b'
  on-primary-fixed-variant: '#003ea8'
  secondary-fixed: '#d3e4fe'
  secondary-fixed-dim: '#b7c8e1'
  on-secondary-fixed: '#0b1c30'
  on-secondary-fixed-variant: '#38485d'
  tertiary-fixed: '#dae2fd'
  tertiary-fixed-dim: '#bec6e0'
  on-tertiary-fixed: '#131b2e'
  on-tertiary-fixed-variant: '#3f465c'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  headline-lg:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '700'
    lineHeight: '1.2'
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Inter
    fontSize: 30px
    fontWeight: '600'
    lineHeight: '1.3'
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.3'
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.5'
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.5'
  label-lg:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: '1'
    letterSpacing: 0.05em
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: '1'
  label-sm:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '500'
    lineHeight: '1'
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 4px
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 32px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 48px
---

## Brand & Style
This design system is built for the high-performance SaaS landscape, where efficiency meets approachability. The brand personality is rooted in reliability and precision—the "Industrial" aspect—but executed with a "Clean & Airy" aesthetic that prevents user fatigue. 

The target audience consists of professionals who require high data density without the clutter. The emotional response should be one of "effortless control." We achieve this through a Corporate/Modern style that leans heavily into minimalism, utilizing generous whitespace and a strictly functional color application to guide the user's focus toward their primary tasks.

## Colors
The palette shifts away from the saturated, dark aesthetics of typical AI tools toward a balanced, light-first environment. 

- **Primary:** A sophisticated Blue (#2563EB) serves as the primary action color, providing a clear signal for interactive elements.
- **Surface Strategy:** We use a layered approach with pure white (#FFFFFF) for primary content containers and off-white (#F8FAFC) for the canvas or background, creating a subtle but perceptible hierarchy of depth.
- **Typography & Borders:** Text is anchored in Charcoal/Slate (#1E293B) to ensure AAA accessibility. Accents and secondary information use softer Slates (#64748B) to reduce visual noise.

## Typography
Inter is the sole typeface for this design system, chosen for its exceptional legibility in technical interfaces and its neutral, systematic character.

- **Headlines:** Use tighter letter spacing and heavier weights to create strong visual anchors.
- **Body:** Standardized at 16px for optimal readability across desktop and mobile. 
- **Labels:** Utilized for metadata and small buttons; uppercase should be reserved for the `label-lg` tier to assist in categorizing sections without adding weight.

## Layout & Spacing
This design system utilizes a **12-column fluid grid** for desktop and a **4-column fluid grid** for mobile. The layout is built on an 8px rhythmic scale to maintain vertical consistency.

- **Desktop:** 24px gutters with 48px outer margins. Content containers should typically max out at 1440px to maintain line-length readability.
- **Mobile:** 16px gutters and margins.
- **Rhythm:** All internal component spacing (padding, gaps) must be a multiple of the 4px base unit, favoring 8px (sm) and 16px (md) for the majority of UI patterns.

## Elevation & Depth
Depth is conveyed primarily through **tonal layers** and **low-contrast outlines** rather than heavy shadows. This maintains the "Airy" feel requested.

1.  **Level 0 (Canvas):** #F8FAFC - The base background.
2.  **Level 1 (Cards/Containers):** #FFFFFF - Elevated slightly via a 1px border (#E2E8F0) to separate from the canvas.
3.  **Level 2 (Dropdowns/Modals):** #FFFFFF - Uses a soft, ambient shadow: `0px 10px 15px -3px rgba(0, 0, 0, 0.05)`.

Avoid using shadows on standard buttons or cards; use borders to define shape and color to define hierarchy.

## Shapes
This design system follows a consistent 8px (0.5rem) corner radius for most UI elements. This "Rounded" setting strikes a balance between the rigid "Industrial" look and a modern, approachable feel.

- **Base Radius:** 8px (Buttons, Input fields, Chips).
- **Large Radius:** 16px (Cards, Modals, Section containers).
- **Full Radius:** 9999px (Status indicators, Search bars).

## Components
- **Buttons:** Primary buttons use a solid Blue (#2563EB) background with White text. Secondary buttons use a Slate-100 background or a simple 1px border with Slate-800 text.
- **Inputs:** High-contrast borders (#CBD5E1) that transition to the Primary Blue on focus. Labels should always be visible above the field in `label-md`.
- **Chips:** Soft-filled backgrounds (e.g., 10% opacity of the accent color) with 8px rounded corners to denote categories or tags.
- **Cards:** White backgrounds, 1px #E2E8F0 border, 16px corner radius. No shadow unless the card is interactive (hover state).
- **Data Tables:** Row-based layouts with #F8FAFC zebra striping or simple 1px dividers. Header text uses `label-md` in Slate-500.