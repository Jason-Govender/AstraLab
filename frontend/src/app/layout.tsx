import type { Metadata } from "next";
import { AppThemeProvider } from "@/components/AppThemeProvider";
import "./globals.css";

export const metadata: Metadata = {
  title: "AstraLab",
  description: "Profile datasets, analyze data, and run ML workflows in one workspace.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        <AppThemeProvider>{children}</AppThemeProvider>
      </body>
    </html>
  );
}
