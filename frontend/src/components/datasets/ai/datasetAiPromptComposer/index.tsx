"use client";

import { useState } from "react";
import { Button, Card, Input, Typography } from "antd";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;
const { TextArea } = Input;

interface DatasetAiPromptComposerProps {
  datasetVersionId?: number;
  activeConversationId?: number;
  isSubmitting?: boolean;
  onSubmit: (question: string) => Promise<void>;
}

export const DatasetAiPromptComposer = ({
  datasetVersionId,
  activeConversationId,
  isSubmitting = false,
  onSubmit,
}: DatasetAiPromptComposerProps) => {
  const { styles } = useStyles();
  const [question, setQuestion] = useState("");

  const handleSubmit = async () => {
    if (!question.trim()) {
      return;
    }

    await onSubmit(question.trim());
    setQuestion("");
  };

  return (
    <Card className={styles.card}>
      <Title level={4}>Ask about this dataset</Title>
      <Paragraph className={styles.helperText}>
        Ask a question in plain language. The assistant will stay grounded in
        the selected dataset version and continue the current thread when one is
        open.
      </Paragraph>
      <TextArea
        rows={4}
        value={question}
        disabled={!datasetVersionId || isSubmitting}
        placeholder={
          activeConversationId
            ? "Ask a follow-up question about this dataset version"
            : "Ask a question about this dataset version"
        }
        onChange={(event) => setQuestion(event.target.value)}
      />
      <div className={styles.actions}>
        <Button
          type="primary"
          loading={isSubmitting}
          disabled={!datasetVersionId || !question.trim()}
          onClick={() => void handleSubmit()}
        >
          Send question
        </Button>
      </div>
    </Card>
  );
};
