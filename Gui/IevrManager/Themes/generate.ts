#!/usr/bin/env bun
// SPDX-License-Identifier: Apache-2.0
// Emits the IEVR Mod Manager WPF palettes (Light, Dark and the seeded variants) from the Rust ThemeExport
// (aphrody_m3_tokens::theme via `bunos-design theme`, read through @aphrody/m3-os-themes). Styles.xaml holds the control
// styles and reads every colour through DynamicResource, so a palette dictionary only carries roles.
//
//   bun apps/iecode-dotnet/Gui/IevrManager/Themes/generate.ts [--check] [--bin <bunos-design>]
import { readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import {
  productSeed,
  rolesFor,
  rustThemeExport,
  type Mode,
  type ThemeInput,
} from "@aphrody/m3-os-themes";

type Variant = NonNullable<ThemeInput["variant"]>;
interface Palette {
  name: string;
  seed: string;
  variant: Variant;
  mode: Mode;
}

const dir = import.meta.dir;
const root = join(dir, "..", "..", "..", "..", "..");
const args = process.argv.slice(2);
const check = args.includes("--check");
const binAt = args.indexOf("--bin");
const bin =
  binAt >= 0
    ? args[binAt + 1]!
    : (process.env["BUNOS_DESIGN"] ?? "bunos-design");

const product = productSeed(root);
// Light and Dark follow the product seed; the named variants keep their historical hue as a seed, never a hand palette.
const PALETTES: Palette[] = [
  { name: "Light", seed: product, variant: "tonalSpot", mode: "light" },
  { name: "Dark", seed: product, variant: "tonalSpot", mode: "dark" },
  { name: "Red", seed: "#ff2e2e", variant: "fidelity", mode: "dark" },
  { name: "Blue", seed: "#1f6feb", variant: "fidelity", mode: "dark" },
  { name: "Purple", seed: "#a855f7", variant: "fidelity", mode: "dark" },
  { name: "Neon", seed: "#00ffff", variant: "vibrant", mode: "dark" },
  { name: "Pastel", seed: "#a8d5e2", variant: "tonalSpot", mode: "light" },
  { name: "Christmas", seed: "#e23b3b", variant: "fruitSalad", mode: "dark" },
];

/** Extended success/warning roles from the generated Sheet::Tailwind until the ThemeExport carries them. */
function extendedRoles(mode: Mode): Record<string, string> {
  const css = readFileSync(
    join(root, "packages/m3-theme/tailwind.css"),
    "utf8",
  );
  const block =
    mode === "light"
      ? /\n:where\(:root\) \{([^}]*)\}/.exec(css)
      : /\n:where\(:root\.dark, \.dark\) \{([^}]*)\}/.exec(css);
  if (!block)
    throw new Error(
      `packages/m3-theme/tailwind.css: no ${mode} extended roles block`,
    );
  const out: Record<string, string> = {};
  for (const m of block[1]!.matchAll(
    /--md-ext-color-([a-z-]+): (#[\da-f]{6});/gi,
  ))
    out[m[1]!.replace(/-([a-z])/g, (_, c: string) => c.toUpperCase())] =
      m[2]!.toLowerCase();
  return out;
}

const STATE = {
  hover: 0.08,
  focus: 0.1,
  pressed: 0.1,
  disabledContainer: 0.12,
  disabledContent: 0.38,
} as const;

/** An M3 state layer: `layer` at `opacity` composited over `base` (CSS color-mix in srgb). */
function over(layer: string, opacity: number, base: string): string {
  const ch = (hex: string, i: number) =>
    Number.parseInt(hex.slice(1 + i * 2, 3 + i * 2), 16);
  return `#${[0, 1, 2]
    .map((i) =>
      Math.round(ch(layer, i) * opacity + ch(base, i) * (1 - opacity)),
    )
    .map((v) => v.toString(16).padStart(2, "0"))
    .join("")}`;
}

function palette(p: Palette): string {
  const input: ThemeInput = {
    name: p.name,
    seed: p.seed,
    variant: p.variant,
    contrastLevel: 0,
    mode: p.mode,
    nativeTheme: rustThemeExport(p.seed, p.variant, 0, bin),
  };
  const r = rolesFor(input, p.mode).roles;
  const x = extendedRoles(p.mode);
  const role = (k: string): string => {
    const v = r[k] ?? x[k];
    if (!v) throw new Error(`role ${k} missing for ${p.name}`);
    return v;
  };
  const colors: [string, string, string][] = [
    ["BgColor", role("surface"), "surface"],
    ["CardBg", role("surfaceContainer"), "surface-container"],
    [
      "HoverBg",
      over(role("onSurface"), STATE.hover, role("surfaceContainer")),
      "on-surface 8% over surface-container",
    ],
    [
      "FocusBg",
      over(role("onSurface"), STATE.focus, role("surfaceContainer")),
      "on-surface 10% over surface-container",
    ],
    [
      "PressedBg",
      over(role("onSurface"), STATE.pressed, role("surfaceContainer")),
      "on-surface 10% over surface-container",
    ],
    ["SelectedBg", role("secondaryContainer"), "secondary-container"],
    [
      "TrackColor",
      role("surfaceContainerHighest"),
      "surface-container-highest",
    ],
    [
      "TrackHover",
      over(role("onSurface"), STATE.hover, role("surfaceContainerHighest")),
      "on-surface 8% over surface-container-highest",
    ],
    ["BorderColor", role("outlineVariant"), "outline-variant"],
    ["OutlineColor", role("outline"), "outline"],
    ["FocusRingColor", role("secondary"), "secondary"],
    ["TextColor", role("onSurface"), "on-surface"],
    ["TextBrightColor", role("onSurface"), "on-surface"],
    ["TimestampColor", role("onSurfaceVariant"), "on-surface-variant"],
    ["AccentColor", role("primary"), "primary"],
    [
      "AccentHover",
      over(role("onPrimary"), STATE.hover, role("primary")),
      "on-primary 8% over primary",
    ],
    [
      "AccentActive",
      over(role("onPrimary"), STATE.pressed, role("primary")),
      "on-primary 10% over primary",
    ],
    ["OnAccentColor", role("onPrimary"), "on-primary"],
    ["SuccessColor", role("success"), "ext success"],
    [
      "SuccessHover",
      over(role("onSuccess"), STATE.hover, role("success")),
      "on-success 8% over success",
    ],
    [
      "SuccessActive",
      over(role("onSuccess"), STATE.pressed, role("success")),
      "on-success 10% over success",
    ],
    ["OnSuccessColor", role("onSuccess"), "ext on-success"],
    ["ErrorColor", role("error"), "error"],
    [
      "ErrorHover",
      over(role("onError"), STATE.hover, role("error")),
      "on-error 8% over error",
    ],
    [
      "ErrorActive",
      over(role("onError"), STATE.pressed, role("error")),
      "on-error 10% over error",
    ],
    ["OnErrorColor", role("onError"), "on-error"],
    ["InfoColor", role("tertiary"), "tertiary"],
    ["WarningColor", role("warning"), "ext warning"],
    [
      "DisabledBg",
      over(role("onSurface"), STATE.disabledContainer, role("surface")),
      "on-surface 12% over surface",
    ],
    [
      "DisabledText",
      over(role("onSurface"), STATE.disabledContent, role("surface")),
      "on-surface 38% over surface",
    ],
  ];
  const brush = (key: string) =>
    (key.endsWith("Color") ? key.slice(0, -"Color".length) : key) + "Brush";
  const width = Math.max(...colors.map(([k]) => k.length));
  return `<!-- GENERATED - do not hand-edit. Generated by apps/iecode-dotnet/Gui/IevrManager/Themes/generate.ts.
     Rust ThemeExport (bunos-design theme): seed ${p.seed}, variant ${p.variant}, contrast 0, ${p.mode}. -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="Styles.xaml"/>
    </ResourceDictionary.MergedDictionaries>

${colors.map(([k, v, why]) => `    <Color x:Key="${k}">${v}</Color>${" ".repeat(width - k.length)} <!-- ${why} -->`).join("\n")}

${colors.map(([k]) => `    <SolidColorBrush x:Key="${brush(k)}" Color="{StaticResource ${k}}"/>`).join("\n")}
</ResourceDictionary>
`;
}

let stale = 0;
for (const p of PALETTES) {
  const path = join(dir, `${p.name}.xaml`);
  const next = palette(p);
  let prev = "";
  try {
    prev = readFileSync(path, "utf8");
  } catch (e) {
    if ((e as NodeJS.ErrnoException).code !== "ENOENT") throw e;
  }
  if (prev === next) {
    console.log(`ok    ${p.name}.xaml`);
    continue;
  }
  if (check) {
    console.log(`stale ${p.name}.xaml`);
    stale++;
  } else {
    writeFileSync(path, next);
    console.log(`wrote ${p.name}.xaml`);
  }
}
if (stale) process.exit(1);
