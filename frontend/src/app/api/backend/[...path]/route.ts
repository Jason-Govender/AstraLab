import { proxyToBackend } from "@/lib/api/runtime.js";

export const dynamic = "force-dynamic";

type RouteContext = {
  params: Promise<{
    path: string[];
  }>;
};

async function handle(request: Request, context: RouteContext) {
  const { path } = await context.params;
  return proxyToBackend(request, path ?? []);
}

export { handle as GET };
export { handle as POST };
export { handle as PUT };
export { handle as PATCH };
export { handle as DELETE };
export { handle as HEAD };
export { handle as OPTIONS };
