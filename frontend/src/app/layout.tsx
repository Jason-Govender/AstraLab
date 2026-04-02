import type { Metadata } from "next";
import { AuthBootstrap } from "@/components/AuthBootstrap";
import { AppThemeProvider } from "@/components/AppThemeProvider";
import { AuthProvider } from "@/providers/authProvider";
import "./globals.css";

export const metadata: Metadata = {
  title: "AstraLab",
  description: "Profile datasets, analyze data, and run ML workflows in one workspace.",
  icons: {
    icon: [{ url: "/logo.svg", type: "image/svg+xml" }],
    shortcut: [{ url: "/logo.svg", type: "image/svg+xml" }],
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        <AppThemeProvider>
          <AuthProvider>
            <AuthBootstrap />
            {children}
          </AuthProvider>
        </AppThemeProvider>
      </body>
    </html>
  );
}
