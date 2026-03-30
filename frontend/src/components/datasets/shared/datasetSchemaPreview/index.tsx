"use client";

import { Card, Descriptions, Empty, Tag, Typography } from "antd";
import { parseDatasetSchema } from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetSchemaPreviewProps {
  schemaJson?: string | null;
}

export const DatasetSchemaPreview = ({
  schemaJson,
}: DatasetSchemaPreviewProps) => {
  const { styles } = useStyles();
  const schema = parseDatasetSchema(schemaJson);

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        Schema Preview
      </Title>

      {!schema ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Schema metadata will appear here when available."
        />
      ) : (
        <>
          <Descriptions
            size="small"
            column={1}
            items={[
              {
                key: "format",
                label: "Format",
                children: <Tag>{schema.format}</Tag>,
              },
              {
                key: "root-kind",
                label: "Root kind",
                children: <Tag color="geekblue">{schema.rootKind}</Tag>,
              },
              {
                key: "column-count",
                label: "Columns",
                children: schema.columns.length,
              },
            ]}
          />

          <Paragraph className={styles.helperText}>
            Backend-derived schema extracted during ingestion.
          </Paragraph>

          <div className={styles.list}>
            {schema.columns.map((column) => (
              <div key={`${column.ordinal}-${column.name}`} className={styles.listItem}>
                <div>
                  <div className={styles.listItemName}>{column.name}</div>
                  <div className={styles.listItemMeta}>Ordinal {column.ordinal}</div>
                </div>
                <Tag color={column.isDataTypeInferred ? "processing" : "default"}>
                  {column.dataType || "unknown"}
                </Tag>
              </div>
            ))}
          </div>
        </>
      )}
    </Card>
  );
};
