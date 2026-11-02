import { Link } from "react-router-dom";
import {
  Mail,
  Phone,
  MapPin,
  Facebook,
  Twitter,
  Instagram,
  Linkedin,
  ArrowUpRight,
} from "lucide-react";
import Logo from "@/components/atoms/Logo";
import { t } from "@/i18n";

export default function PublicFooter() {
  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  return (
    <footer id="contact" className="relative bg-slate-900 text-white">
      <div className="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-indigo-500 to-transparent opacity-50"></div>
      
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-16">
        <div className="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-4">
          {/* Brand Section */}
          <div className="col-span-1 lg:col-span-2">
            <div className="flex items-center gap-2 mb-6">
              <Logo className="h-10 w-10 shadow-lg" />
              <span className="text-xl font-bold">{t("CampusERP")}</span>
            </div>
            <p className="text-slate-400 max-w-md mb-6 leading-relaxed">
              {t("Empowering educational institutions with a comprehensive, modern platform for seamless school management. From enrollment to academics, we simplify every aspect of education administration.")}
            </p>
            <div className="flex gap-4">
              <a
                href="https://facebook.com"
                target="_blank"
                rel="noopener noreferrer"
                className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-800 text-slate-400 hover:bg-indigo-500 hover:text-white transition-all duration-300 hover:scale-110"
              >
                <Facebook className="h-5 w-5" />
              </a>
              <a
                href="https://twitter.com"
                target="_blank"
                rel="noopener noreferrer"
                className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-800 text-slate-400 hover:bg-indigo-500 hover:text-white transition-all duration-300 hover:scale-110"
              >
                <Twitter className="h-5 w-5" />
              </a>
              <a
                href="https://instagram.com"
                target="_blank"
                rel="noopener noreferrer"
                className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-800 text-slate-400 hover:bg-indigo-500 hover:text-white transition-all duration-300 hover:scale-110"
              >
                <Instagram className="h-5 w-5" />
              </a>
              <a
                href="https://linkedin.com"
                target="_blank"
                rel="noopener noreferrer"
                className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-800 text-slate-400 hover:bg-indigo-500 hover:text-white transition-all duration-300 hover:scale-110"
              >
                <Linkedin className="h-5 w-5" />
              </a>
            </div>
          </div>

          {/* Quick Links */}
          <div>
            <h3 className="text-lg font-semibold mb-6">{t("Quick Links")}</h3>
            <ul className="space-y-4">
              <li>
                <Link
                  to="/"
                  className="text-slate-400 hover:text-white transition-colors duration-200 flex items-center gap-2 group"
                >
                  <ArrowUpRight className="h-4 w-4 opacity-0 group-hover:opacity-100 transition-opacity" />
                  <span>{t("Home")}</span>
                </Link>
              </li>
              <li>
                <Link
                  to="#features"
                  className="text-slate-400 hover:text-white transition-colors duration-200 flex items-center gap-2 group"
                  onClick={(e) => {
                    e.preventDefault();
                    const element = document.getElementById("features");
                    if (element) {
                      element.scrollIntoView({ behavior: "smooth" });
                    }
                  }}
                >
                  <ArrowUpRight className="h-4 w-4 opacity-0 group-hover:opacity-100 transition-opacity" />
                  <span>{t("Features")}</span>
                </Link>
              </li>
              <li>
                <Link
                  to="/register-school"
                  className="text-slate-400 hover:text-white transition-colors duration-200 flex items-center gap-2 group"
                >
                  <ArrowUpRight className="h-4 w-4 opacity-0 group-hover:opacity-100 transition-opacity" />
                  <span>{t("Register School")}</span>
                </Link>
              </li>
              <li>
                <Link
                  to="/auth/login"
                  className="text-slate-400 hover:text-white transition-colors duration-200 flex items-center gap-2 group"
                >
                  <ArrowUpRight className="h-4 w-4 opacity-0 group-hover:opacity-100 transition-opacity" />
                  <span>{t("Sign In")}</span>
                </Link>
              </li>
              <li>
                <Link
                  to="#about"
                  className="text-slate-400 hover:text-white transition-colors duration-200 flex items-center gap-2 group"
                  onClick={(e) => {
                    e.preventDefault();
                    const element = document.getElementById("about");
                    if (element) {
                      element.scrollIntoView({ behavior: "smooth" });
                    }
                  }}
                >
                  <ArrowUpRight className="h-4 w-4 opacity-0 group-hover:opacity-100 transition-opacity" />
                  <span>{t("About Us")}</span>
                </Link>
              </li>
            </ul>
          </div>

          {/* Contact Info */}
          <div>
            <h3 className="text-lg font-semibold mb-6">{t("Get in Touch")}</h3>
            <ul className="space-y-4">
              <li className="flex items-start gap-3 text-slate-400">
                <MapPin className="h-5 w-5 mt-0.5 text-indigo-400 flex-shrink-0" />
                <span className="leading-relaxed">
                  {t("123 Education Street")}<br />
                  {t("Learning City, LC 12345")}
                </span>
              </li>
              <li className="flex items-center gap-3 text-slate-400">
                <Phone className="h-5 w-5 text-indigo-400 flex-shrink-0" />
                <span>+1 (555) 123-4567</span>
              </li>
              <li className="flex items-center gap-3 text-slate-400">
                <Mail className="h-5 w-5 text-indigo-400 flex-shrink-0" />
                <span>{t("info@schoolmanager.com")}</span>
              </li>
            </ul>
          </div>
        </div>

        {/* Bottom Section */}
        <div className="mt-12 pt-8 border-t border-slate-800">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <p className="text-slate-400 text-sm">
              © {new Date().getFullYear()} {t("CampusERP. All rights reserved.")}
            </p>
            <button
              onClick={scrollToTop}
              className="group flex items-center gap-2 text-sm text-slate-400 hover:text-white transition-colors duration-200 self-start md:self-auto"
            >
              <span>{t("Back to top")}</span>
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-slate-800 group-hover:bg-indigo-500 transition-colors duration-300">
                <ArrowUpRight className="h-4 w-4 rotate-[-45deg]" />
              </div>
            </button>
          </div>
        </div>
      </div>
    </footer>
  );
}