"use client";

import { Typography } from "antd";
import type { WorkspacePageHeaderProps } from "@/types/workspace";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

export function WorkspacePageHeader({
  title,
  description,
  actions,
}: WorkspacePageHeaderProps) {
  const { styles } = useStyles();

  return (
    <div className={styles.pageHeader}>
      <div>
        <Title level={1} className={styles.pageHeaderTitle}>
          {title}
        </Title>
        <Paragraph className={styles.pageHeaderDescription}>
          {description}
        </Paragraph>
      </div>

      {actions ?? null}
    </div>
  );
}
