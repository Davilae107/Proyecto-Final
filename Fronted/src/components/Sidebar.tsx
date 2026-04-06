"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import {
  LayoutDashboard,
  Upload,
  BarChart3,
  Database,
  GraduationCap,
  MapPin,
  FileText,
  Activity,
  ChevronLeft,
  ChevronRight,
  Brain,
  LogOut,
} from "lucide-react";
import { useMemo, useState } from "react";
import { clearAuthSession, getAuthUser } from "@/lib/auth";

const navItems = [
  {
    label: "Dashboard",
    href: "/dashboard",
    icon: LayoutDashboard,
  },
  {
    label: "Subir CSV",
    href: "/dashboard/csv-upload",
    icon: Upload,
  },
  {
    label: "Pipeline ETL",
    href: "/dashboard/etl",
    icon: Database,
  },
  {
    label: "Módulo IA",
    href: "/dashboard/ia",
    icon: Brain,
  },
  {
    label: "Rendimiento Académico",
    href: "/dashboard/estadisticas/rendimiento",
    icon: GraduationCap,
  },
  {
    label: "Deserción Escolar",
    href: "/dashboard/estadisticas/desercion",
    icon: Activity,
  },
  {
    label: "Indicadores Socioeconómicos",
    href: "/dashboard/estadisticas/socioeconomicos",
    icon: BarChart3,
  },
  {
    label: "Cobertura Educativa",
    href: "/dashboard/estadisticas/cobertura",
    icon: MapPin,
  },
  {
    label: "Servicios Básicos",
    href: "/dashboard/estadisticas/servicios",
    icon: FileText,
  },
];

export default function Sidebar() {
  const pathname = usePathname();
  const router = useRouter();
  const [collapsed, setCollapsed] = useState(false);
  const authUser = useMemo(() => getAuthUser(), []);

  const handleLogout = () => {
    clearAuthSession();
    router.push("/login");
    router.refresh();
  };

  return (
    <aside
      className={`${
        collapsed ? "w-16" : "w-64"
      } bg-white border-r border-gray-200 h-screen sticky top-0 flex flex-col transition-all duration-300 shadow-sm`}
    >
      {/* Logo */}
      <div className="h-16 flex items-center justify-between px-4 border-b border-gray-200">
        {!collapsed && (
          <Link href="/dashboard" className="flex items-center gap-2">
            <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
              <GraduationCap className="w-5 h-5 text-white" />
            </div>
            <span className="font-bold text-lg text-blue-600">SIPAD</span>
          </Link>
        )}
        <button
          onClick={() => setCollapsed(!collapsed)}
          className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500"
        >
          {collapsed ? (
            <ChevronRight className="w-4 h-4" />
          ) : (
            <ChevronLeft className="w-4 h-4" />
          )}
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 py-4 px-2 space-y-1 overflow-y-auto">
        {navItems.map((item) => {
          const isActive =
            pathname === item.href ||
            (item.href !== "/dashboard" && pathname.startsWith(item.href));
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? "bg-blue-50 text-blue-700 border border-blue-200"
                  : "text-gray-600 hover:bg-gray-100 hover:text-gray-900"
              }`}
              title={collapsed ? item.label : undefined}
            >
              <item.icon
                className={`w-5 h-5 shrink-0 ${
                  isActive ? "text-blue-600" : "text-gray-400"
                }`}
              />
              {!collapsed && <span>{item.label}</span>}
            </Link>
          );
        })}
      </nav>

      {/* Footer */}
      {!collapsed && (
        <div className="p-4 border-t border-gray-200">
          {authUser && (
            <p className="mb-3 text-xs text-gray-500 truncate" title={authUser.email}>
              {authUser.fullName || authUser.email}
            </p>
          )}
          <button
            type="button"
            onClick={handleLogout}
            className="mb-3 w-full inline-flex items-center justify-center gap-2 rounded-lg border border-gray-200 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100"
          >
            <LogOut className="h-4 w-4" />
            Cerrar sesión
          </button>
          <p className="text-xs text-gray-400 text-center">
            SIPAD v1.0 &copy; 2024
          </p>
        </div>
      )}
    </aside>
  );
}
