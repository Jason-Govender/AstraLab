import type { PropsWithChildren } from "react";
import { WorkspaceShell } from "@/components/workspaceShell/WorkspaceShell";

export default function ProtectedLayout({ children }: PropsWithChildren) {
  return <WorkspaceShell>{children}</WorkspaceShell>;
}
