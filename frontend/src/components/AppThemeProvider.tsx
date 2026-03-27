"use client";

import type { PropsWithChildren } from "react";
import { App, ConfigProvider } from "antd";
import { ThemeProvider } from "antd-style";
import { appTheme } from "@/styles";

export function AppThemeProvider({ children }: PropsWithChildren) {
  return (
    <ConfigProvider theme={appTheme}>
      <ThemeProvider appearance="dark" theme={appTheme}>
        <App>{children}</App>
      </ThemeProvider>
    </ConfigProvider>
  );
}
