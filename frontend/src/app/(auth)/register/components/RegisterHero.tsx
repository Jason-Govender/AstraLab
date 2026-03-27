import { Card, Typography } from "antd";
import { useStyles } from "../style";

const { Paragraph, Text, Title } = Typography;

interface RegisterHeroProps {
  benefits: string[];
}

export function RegisterHero({ benefits }: RegisterHeroProps) {
  const { styles } = useStyles();

  return (
    <div className={styles.hero}>
      <Text className={styles.brand}>AstraLab</Text>
      <Title level={1} className={styles.heroTitle}>
        Build your data
        <span className={styles.heroAccent}> intelligence workspace</span>
      </Title>
      <Paragraph className={styles.heroDescription}>
        Create projects, upload datasets, run analyses, chat with AI, and
        generate stakeholder-ready reports.
      </Paragraph>

      <Card bordered className={styles.benefitCard}>
        <Title level={4} className={styles.benefitTitle}>
          What you get
        </Title>
        <ul className={styles.benefitList}>
          {benefits.map((benefit) => (
            <li key={benefit} className={styles.benefitItem}>
              {benefit}
            </li>
          ))}
        </ul>
      </Card>
    </div>
  );
}
