import { Link, useNavigate } from "react-router-dom";
import { Menu, X, LogIn } from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";
import Logo from "@/components/atoms/Logo";
import LanguageSwitcher from "@/components/molecules/LanguageSwitcher";
import { t } from "@/i18n";

export default function PublicNavbar() {
  const [isOpen, setIsOpen] = useState(false);
  const navigate = useNavigate();

  return (
    <header className="fixed top-0 left-0 right-0 z-50 bg-white/80 dark:bg-slate-900/80 backdrop-blur-lg border-b border-slate-200/60 dark:border-slate-700/60">
      <nav className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          <Link to="/" className="flex items-center gap-2 group">
            <Logo className="h-10 w-10 shadow-lg transition-all duration-300 group-hover:scale-105" />
            <span className="text-xl font-bold bg-gradient-to-r from-indigo-600 to-violet-600 bg-clip-text text-transparent">
              {t("CampusERP")}
            </span>
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex md:items-center md:gap-8">
            <Link
              to="/"
              className="text-sm font-medium text-slate-700 dark:text-slate-300 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors duration-200"
            >
              {t("Home")}
            </Link>
            <Link
              to="#features"
              className="text-sm font-medium text-slate-700 dark:text-slate-300 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors duration-200"
              onClick={(e) => {
                e.preventDefault();
                const element = document.getElementById("features");
                if (element) {
                  element.scrollIntoView({ behavior: "smooth" });
                }
              }}
            >
              {t("Features")}
            </Link>
            <Link
              to="#about"
              className="text-sm font-medium text-slate-700 dark:text-slate-300 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors duration-200"
              onClick={(e) => {
                e.preventDefault();
                const element = document.getElementById("about");
                if (element) {
                  element.scrollIntoView({ behavior: "smooth" });
                }
              }}
            >
              {t("About")}
            </Link>
            <Link
              to="#contact"
              className="text-sm font-medium text-slate-700 dark:text-slate-300 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors duration-200"
              onClick={(e) => {
                e.preventDefault();
                const element = document.getElementById("contact");
                if (element) {
                  element.scrollIntoView({ behavior: "smooth" });
                }
              }}
            >
              {t("Contact")}
            </Link>
            <button
              onClick={() => navigate("/register-school")}
              className="text-sm font-semibold text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 transition-colors duration-200"
            >
              {t("Get Started")}
            </button>
            <LanguageSwitcher className="text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800" />
            <button
              onClick={() => navigate("/auth/login")}
              className="group inline-flex items-center gap-2 rounded-xl bg-gradient-to-r from-indigo-500 to-violet-500 px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-105 transition-all duration-300"
            >
              <LogIn className="h-4 w-4" />
              {t("Sign In")}
            </button>
          </div>

          {/* Mobile menu button */}
          <button
            type="button"
            aria-label={t("Toggle navigation")}
            className="inline-flex items-center justify-center rounded-lg p-2 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 md:hidden transition-colors"
            onClick={() => setIsOpen(!isOpen)}
          >
            {isOpen ? (
              <X className="h-6 w-6" />
            ) : (
              <Menu className="h-6 w-6" />
            )}
          </button>
        </div>

        {/* Mobile Navigation */}
        {isOpen && (
          <div className="md:hidden animate-fade-in-up">
            <div className="space-y-1 px-2 pb-3 pt-2">
              <Link
                to="/"
                className="block rounded-lg px-3 py-2 text-base font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                onClick={() => setIsOpen(false)}
              >
                {t("Home")}
              </Link>
              <Link
                to="#features"
                className="block rounded-lg px-3 py-2 text-base font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                onClick={(e) => {
                  e.preventDefault();
                  setIsOpen(false);
                  const element = document.getElementById("features");
                  if (element) {
                    element.scrollIntoView({ behavior: "smooth" });
                  }
                }}
              >
                {t("Features")}
              </Link>
              <Link
                to="#about"
                className="block rounded-lg px-3 py-2 text-base font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                onClick={(e) => {
                  e.preventDefault();
                  setIsOpen(false);
                  const element = document.getElementById("about");
                  if (element) {
                    element.scrollIntoView({ behavior: "smooth" });
                  }
                }}
              >
                {t("About")}
              </Link>
              <Link
                to="#contact"
                className="block rounded-lg px-3 py-2 text-base font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                onClick={(e) => {
                  e.preventDefault();
                  setIsOpen(false);
                  const element = document.getElementById("contact");
                  if (element) {
                    element.scrollIntoView({ behavior: "smooth" });
                  }
                }}
              >
                {t("Contact")}
              </Link>
              <button
                onClick={() => {
                  setIsOpen(false);
                  navigate("/register-school");
                }}
                className="mt-2 flex w-full items-center justify-center gap-2 rounded-xl border border-indigo-500/40 px-4 py-2.5 text-sm font-semibold text-indigo-600 dark:text-indigo-400"
              >
                {t("Register School")}
              </button>
              <LanguageSwitcher className="mt-2 w-full justify-center border border-slate-200 py-2.5 dark:border-slate-700" />
              <button
                onClick={() => {
                  setIsOpen(false);
                  navigate("/auth/login");
                }}
                className="mt-2 flex w-full items-center justify-center gap-2 rounded-xl bg-gradient-to-r from-indigo-500 to-violet-500 px-4 py-2.5 text-sm font-semibold text-white"
              >
                <LogIn className="h-4 w-4" />
                {t("Sign In")}
              </button>
            </div>
          </div>
        )}
      </nav>
    </header>
  );
}