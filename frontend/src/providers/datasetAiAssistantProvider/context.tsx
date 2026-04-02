import { createContext } from "react";
import type {
  AIConversation,
  AIResponse,
  AskDatasetAiQuestionRequest,
  GetDatasetAiConversationsRequest,
  GetDatasetAiResponsesRequest,
} from "@/types/datasets";
import type { MlExperiment } from "@/types/ml";

export interface IDatasetAiAssistantAskRequest
  extends AskDatasetAiQuestionRequest {
  mlExperimentId?: number;
}

export interface IDatasetAiAssistantStateContext {
  activeDatasetId?: number;
  activeDatasetVersionId?: number;
  activeConversationId?: number;
  activeMlExperimentId?: number;
  activeExperiment?: MlExperiment;
  conversations: AIConversation[];
  responses: AIResponse[];
  conversationTotalCount: number;
  responseTotalCount: number;
  lastGeneratedResponse?: AIResponse;
  isLoadingExperimentContext: boolean;
  isLoadingConversations: boolean;
  isLoadingResponses: boolean;
  isGenerating: boolean;
  isPending: boolean;
  isError: boolean;
  experimentContextErrorMessage?: string;
  conversationErrorMessage?: string;
  responseErrorMessage?: string;
  generationErrorMessage?: string;
}

export interface IDatasetAiAssistantActionContext {
  loadExperimentContext: (experimentId?: number) => Promise<void>;
  generateSummary: (datasetVersionId: number) => Promise<void>;
  generateInsights: (datasetVersionId: number) => Promise<void>;
  generateCleaningRecommendations: (datasetVersionId: number) => Promise<void>;
  askQuestion: (request: IDatasetAiAssistantAskRequest) => Promise<void>;
  getConversations: (request: GetDatasetAiConversationsRequest) => Promise<void>;
  getResponses: (request: GetDatasetAiResponsesRequest) => Promise<void>;
  setActiveConversation: (conversationId?: number) => void;
  clearConversationState: () => void;
}

export const INITIAL_STATE: IDatasetAiAssistantStateContext = {
  conversations: [],
  responses: [],
  conversationTotalCount: 0,
  responseTotalCount: 0,
  isLoadingExperimentContext: false,
  isLoadingConversations: false,
  isLoadingResponses: false,
  isGenerating: false,
  isPending: false,
  isError: false,
};

export const DatasetAiAssistantStateContext =
  createContext<IDatasetAiAssistantStateContext | undefined>(undefined);

export const DatasetAiAssistantActionContext =
  createContext<IDatasetAiAssistantActionContext | undefined>(undefined);
