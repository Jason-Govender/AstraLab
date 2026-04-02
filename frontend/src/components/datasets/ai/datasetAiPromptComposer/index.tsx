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
  onInteraction?: () => void;
  onSubmit: (question: string) => Promise<boolean>;
}

export const DatasetAiPromptComposer = ({
  datasetVersionId,
  activeConversationId,
  isSubmitting = false,
  onInteraction,
  onSubmit,
}: DatasetAiPromptComposerProps) => {
  const { styles } = useStyles();
  const [question, setQuestion] = useState("");

  const handleSubmit = async () => {
    if (!question.trim()) {
      return;
    }

    const didSubmit = await onSubmit(question.trim());

    if (didSubmit) {
      setQuestion("");
    }
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
        onChange={(event) => {
          onInteraction?.();
          setQuestion(event.target.value);
        }}
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
