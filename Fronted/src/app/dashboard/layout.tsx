"use client";

import Sidebar from "@/components/Sidebar";
import { getAuthToken } from "@/lib/auth";
import { useRouter } from "next/navigation";
import { useEffect, useMemo } from "react";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const token = useMemo(() => getAuthToken(), []);

  useEffect(() => {
    if (!token) {
      router.replace("/login");
    }
  }, [router, token]);

  if (!token) {
    return (
      <div className="min-h-screen flex items-center justify-center text-slate-600">
        Validando sesión...
      </div>
    );
  }

  return (
    <div className="flex min-h-screen">
      <Sidebar />
      <main className="flex-1 overflow-auto">
        <div className="p-6 max-w-7xl mx-auto">{children}</div>
      </main>
    </div>
  );
}
