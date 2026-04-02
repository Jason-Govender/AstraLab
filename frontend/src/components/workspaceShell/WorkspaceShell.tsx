"use client";

import type { ReactNode } from "react";
import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Avatar,
  Button,
  Drawer,
  Dropdown,
  Input,
  Spin,
  Typography,
} from "antd";
import type { MenuProps } from "antd";
import {
  DashboardOutlined,
  DatabaseOutlined,
  ExperimentOutlined,
  FileTextOutlined,
  LogoutOutlined,
  MenuOutlined,
  RobotOutlined,
  SearchOutlined,
} from "@ant-design/icons";
import { BrandLogo } from "@/components/BrandLogo";
import { WORKSPACE_NAVIGATION } from "@/constants/workspace";
import { useAuthActions, useAuthState } from "@/providers/authProvider";
import type {
  WorkspaceIconKey,
  WorkspaceNavItem,
  WorkspaceShellProps,
} from "@/types/workspace";
import { useStyles } from "./style";

const { Text } = Typography;

const ICON_MAP: Record<WorkspaceIconKey, ReactNode> = {
  dashboard: <DashboardOutlined />,
  datasets: <DatabaseOutlined />,
  assistant: <RobotOutlined />,
  mlWorkspace: <ExperimentOutlined />,
  reports: <FileTextOutlined />,
};

function getUserInitials(name: string) {
  const segments = name
    .split(" ")
    .map((value) => value.trim())
    .filter(Boolean)
    .slice(0, 2);

  if (segments.length === 0) {
    return "AL";
  }

  return segments.map((value) => value[0]?.toUpperCase() ?? "").join("");
}

function isItemSelected(pathname: string, item: WorkspaceNavItem) {
  return pathname === item.href || pathname.startsWith(`${item.href}/`);
}

export function WorkspaceShell({ children }: WorkspaceShellProps) {
  const { styles, cx } = useStyles();
  const pathname = usePathname();
  const { logout } = useAuthActions();
  const { isAuthenticated, isInitialized, profile } = useAuthState();
  const [isMobileNavigationOpen, setIsMobileNavigationOpen] = useState(false);

  const displayName =
    profile?.user?.name || profile?.user?.userName || "AstraLab User";
  const emailAddress = profile?.user?.emailAddress || "Authenticated session";
  const initials = getUserInitials(displayName);

  const profileMenuItems: MenuProps["items"] = [
    {
      key: "profile",
      disabled: true,
      label: (
        <div className={styles.dropdownMeta}>
          <span className={styles.dropdownTitle}>{displayName}</span>
          <span className={styles.dropdownText}>{emailAddress}</span>
        </div>
      ),
    },
    {
      type: "divider",
    },
    {
      key: "logout",
      icon: <LogoutOutlined />,
      label: "Sign out",
    },
  ];

  const navigationContent = (
    <div className={styles.mobileDrawerContent}>
      <div className={styles.brand}>
        <BrandLogo variant="sidebar" />
      </div>

      <section className={styles.navigationSection}>
        <Text className={styles.sectionLabel}>Workspace</Text>

        <nav className={styles.navigationList} aria-label="Workspace navigation">
          {WORKSPACE_NAVIGATION.map((item) => {
            const isSelected = isItemSelected(pathname, item);
            const className = cx(
              styles.navItem,
              isSelected && styles.navItemActive,
              !item.isAvailable && styles.navItemDisabled,
            );

            if (!item.isAvailable) {
              return (
                <div key={item.key} className={className} aria-disabled="true">
                  <span className={styles.navIcon}>{ICON_MAP[item.icon]}</span>
                  <span>{item.label}</span>
                </div>
              );
            }

            return (
              <Link
                key={item.key}
                href={item.href}
                className={className}
                aria-current={isSelected ? "page" : undefined}
                onClick={() => setIsMobileNavigationOpen(false)}
              >
                <span className={styles.navIcon}>{ICON_MAP[item.icon]}</span>
                <span>{item.label}</span>
              </Link>
            );
          })}
        </nav>
      </section>
    </div>
  );

  if (!isInitialized) {
    return (
      <main className={styles.loadingState}>
        <Spin size="large" />
      </main>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <main className={styles.page}>
      <div className={styles.frame}>
        <aside className={styles.sidebar}>{navigationContent}</aside>

        <section className={styles.contentColumn}>
          <header className={styles.header}>
            <Button
              type="default"
              icon={<MenuOutlined />}
              className={styles.mobileMenuButton}
              onClick={() => setIsMobileNavigationOpen(true)}
              aria-label="Open navigation"
            />

            <div className={styles.mobileBrand}>
              <BrandLogo variant="header" />
            </div>

            <Input
              size="large"
              placeholder="Search datasets, reports, experiments..."
              prefix={<SearchOutlined />}
              className={styles.search}
              aria-label="Search workspace"
            />

            <div className={styles.headerUtilities}>
              <Dropdown
                menu={{
                  items: profileMenuItems,
                  onClick: ({ key }) => {
                    if (key === "logout") {
                      logout();
                    }
                  },
                }}
                trigger={["click"]}
              >
                <Button className={styles.profileButton} aria-label="Open profile menu">
                  <Avatar size={36} className={styles.avatar}>
                    {initials}
                  </Avatar>
                </Button>
              </Dropdown>
            </div>
          </header>

          <div className={styles.contentViewport}>
            <div className={styles.contentInner}>{children}</div>
          </div>
        </section>
      </div>

      <Drawer
        placement="left"
        width={280}
        open={isMobileNavigationOpen}
        onClose={() => setIsMobileNavigationOpen(false)}
        title="Workspace"
      >
        {navigationContent}
      </Drawer>
    </main>
  );
}
