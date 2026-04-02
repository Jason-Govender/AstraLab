import Image from "next/image";
import type { CSSProperties } from "react";

type BrandLogoVariant = "auth" | "sidebar" | "header";

interface BrandLogoVariantConfig {
  width: number;
  height: number;
  wrapperStyle: CSSProperties;
}

interface BrandLogoProps {
  variant?: BrandLogoVariant;
  priority?: boolean;
}

const BRAND_LOGO_VARIANTS: Record<BrandLogoVariant, BrandLogoVariantConfig> = {
  auth: {
    width: 207,
    height: 42,
    wrapperStyle: {
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      padding: "12px 16px",
      borderRadius: 20,
      border: "1px solid rgba(255, 255, 255, 0.68)",
      background: "rgba(255, 255, 255, 0.98)",
      boxShadow: "0 20px 42px rgba(0, 0, 0, 0.18)",
    },
  },
  sidebar: {
    width: 156,
    height: 32,
    wrapperStyle: {
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      padding: "10px 12px",
      borderRadius: 18,
      border: "1px solid rgba(255, 255, 255, 0.6)",
      background: "rgba(255, 255, 255, 0.98)",
      boxShadow: "0 14px 30px rgba(0, 0, 0, 0.18)",
    },
  },
  header: {
    width: 132,
    height: 26,
    wrapperStyle: {
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      padding: "8px 12px",
      borderRadius: 16,
      border: "1px solid rgba(255, 255, 255, 0.54)",
      background: "rgba(255, 255, 255, 0.98)",
      boxShadow: "0 10px 24px rgba(0, 0, 0, 0.18)",
    },
  },
};

const imageStyle: CSSProperties = {
  width: "auto",
  height: "auto",
  maxWidth: "100%",
};

export function BrandLogo({
  variant = "sidebar",
  priority = false,
}: BrandLogoProps) {
  const configuration = BRAND_LOGO_VARIANTS[variant];

  return (
    <span style={configuration.wrapperStyle}>
      <Image
        src="/logo.svg"
        alt="AstraLab logo"
        width={configuration.width}
        height={configuration.height}
        priority={priority}
        style={imageStyle}
      />
    </span>
  );
}
