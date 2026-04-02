import { createContext } from "react";
import type {
  AIConversation,
  AIResponse,
  AskDatasetAiQuestionRequest,
  GetDatasetAiConversationsRequest,
  GetDatasetAiResponsesRequest,
} from "@/types/datasets";

export interface IDatasetAiAssistantStateContext {
  activeDatasetId?: number;
  activeDatasetVersionId?: number;
  activeConversationId?: number;
  conversations: AIConversation[];
  responses: AIResponse[];
  conversationTotalCount: number;
  responseTotalCount: number;
  lastGeneratedResponse?: AIResponse;
  isLoadingConversations: boolean;
  isLoadingResponses: boolean;
  isGenerating: boolean;
  isPending: boolean;
  isError: boolean;
  conversationErrorMessage?: string;
  responseErrorMessage?: string;
  generationErrorMessage?: string;
}

export interface IDatasetAiAssistantActionContext {
  generateSummary: (datasetVersionId: number) => Promise<void>;
  generateInsights: (datasetVersionId: number) => Promise<void>;
  generateCleaningRecommendations: (datasetVersionId: number) => Promise<void>;
  askQuestion: (request: AskDatasetAiQuestionRequest) => Promise<void>;
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
