# POS hardware matrix (pilot)

Status: **documented for Phase 4**. Test on real devices before promising support.

## Supported for pilot (target)

| Role | Recommendation | Notes |
| --- | --- | --- |
| Android POS | Mid-range Android 10+ phone/tablet | Flutter `clients/mobile` |
| Barcode | USB HID keyboard-wedge scanners; camera scan optional | Web POS uses focused text input |
| Receipt printer | ESC/POS USB or Bluetooth thermal 58/80mm | Browser print CSS for web; native print TBD |
| Cash drawer | Kick via printer pulse where supported | Not automated in MVP |

## Explicitly unverified

- iOS App Store distribution packaging
- Specific Chinese brand printers without ESC/POS
- Electronic scales (grocery weighted) — deferred until hardware validated

## Arabic receipts

Web receipt uses browser print (`PosPage`). Prefer fonts that include Arabic glyphs. Validate RTL on the chosen thermal printer firmware.
