import ErrorBoundary from "@/components/organisms/ErrorBoundary";
import Router from "@/router";

export default function App() {
  return (
    <ErrorBoundary>
      <Router />
    </ErrorBoundary>
  );
}
