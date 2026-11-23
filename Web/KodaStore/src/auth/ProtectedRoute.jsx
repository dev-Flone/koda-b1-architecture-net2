import { Navigate, useLocation } from "react-router";
import { useAuth } from "../hooks/useAuth";

const ProtectedRoute = ({ children }) => {
  const { status, isAuthenticated } = useAuth();

  const location = useLocation();

  if (status === "checking") {
    return (
      <main className="flex min-h-screen items-center justify-center bg-slate-950">
        <div className="text-center text-white">
          <div className="mx-auto mb-4 size-10 animate-spin rounded-full border-4 border-slate-600 border-t-orange-500" />

          <p className="text-sm">Checking user's session...</p>
        </div>
      </main>
    );
  }
  if (!isAuthenticated)
    return <Navigate to="/login" replace state={{ from: location }} />;

  return children;
};

export default ProtectedRoute;
