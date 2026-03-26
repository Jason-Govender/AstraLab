import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "AstraLab API Console",
  description: "Verify local CORS and deployed proxy communication with the AstraLab ASP.NET Core API.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
