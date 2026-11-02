import {
  GraduationCap,
  Users,
  BookOpen,
  Calendar,
  Award,
  ShieldCheck,
  ArrowRight,
  CheckCircle,
  Sparkles,
  Zap,
  Globe,
  BarChart3,
  Star,
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import Logo from "@/components/atoms/Logo";
import { t } from "@/i18n";
import { usePublicPlans } from "@/features/billing/hooks";
import {
  localizedPlanDescription,
  planHighlightLabels,
} from "@/features/billing/display";

const features = [
  {
    icon: Users,
    title: t("Student Management"),
    description: t("Effortlessly manage student records, enrollment, attendance, and academic progress all in one place."),
    color: "from-blue-500 to-cyan-500",
  },
  {
    icon: BookOpen,
    title: t("Teacher Portal"),
    description: t("Empower teachers with tools for lesson planning, grading, attendance tracking, and parent communication."),
    color: "from-purple-500 to-pink-500",
  },
  {
    icon: Calendar,
    title: t("Timetable & Scheduling"),
    description: t("Create and manage class schedules, exam timetables, and events with intelligent conflict detection."),
    color: "from-orange-500 to-red-500",
  },
  {
    icon: BarChart3,
    title: t("Analytics & Reports"),
    description: t("Gain valuable insights with comprehensive reports on performance, attendance, and institutional growth."),
    color: "from-emerald-500 to-teal-500",
  },
  {
    icon: ShieldCheck,
    title: t("Secure & Reliable"),
    description: t("Enterprise-grade security with role-based access control to protect sensitive student and staff data."),
    color: "from-indigo-500 to-violet-500",
  },
  {
    icon: Award,
    title: t("Exams & Results"),
    description: t("Streamline exam management, grade calculations, report card generation, and result publication."),
    color: "from-yellow-500 to-orange-500",
  },
];

const stats = [
  { value: "1000+", label: t("Schools Trust Us") },
  { value: "50K+", label: t("Students Managed") },
  { value: "10K+", label: t("Teachers Empowered") },
  { value: "99.9%", label: t("Uptime Guarantee") },
];

const testimonials = [
  {
    name: "Sarah Johnson",
    role: t("School Administrator"),
    content: t("CampusERP has transformed how we run our institution. Everything is organized, efficient, and our staff loves it!"),
    rating: 5,
  },
  {
    name: "Michael Chen",
    role: t("Principal"),
    content: t("The analytics and reporting features give me insights I never had before. Highly recommended for any school."),
    rating: 5,
  },
  {
    name: "Emily Davis",
    role: t("Teacher"),
    content: t("Grading, attendance, and communication with parents has never been easier. A game-changer for educators."),
    rating: 5,
  },
];

export default function LandingPage() {
  const navigate = useNavigate();
  const { data: plans = [], isLoading: plansLoading } = usePublicPlans();

  return (
    <div className="overflow-hidden">
      {/* Hero Section */}
      <section className="relative pt-32 pb-20 md:pt-40 md:pb-28 lg:pt-48 lg:pb-32">
        {/* Background decoration */}
        <div className="absolute inset-0 -z-10">
          <div className="absolute top-20 left-1/2 -translate-x-1/2 w-[800px] h-[800px] bg-gradient-to-br from-indigo-500/20 to-violet-500/20 rounded-full blur-[120px] animate-pulse-glow"></div>
          <div className="absolute top-40 right-20 w-[400px] h-[400px] bg-gradient-to-br from-purple-500/15 to-pink-500/15 rounded-full blur-[100px] animate-float"></div>
          <div className="absolute bottom-20 left-20 w-[300px] h-[300px] bg-gradient-to-br from-cyan-500/15 to-blue-500/15 rounded-full blur-[80px] animate-float" style={{ animationDelay: "3s" }}></div>
        </div>

        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <div className="inline-flex items-center gap-2 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-4 py-1.5 mb-8 animate-fade-in-up">
              <Sparkles className="h-4 w-4 text-indigo-400" />
              <span className="text-sm font-medium text-indigo-300">
                {t("The Future of School Management")}
              </span>
            </div>

            <h1 className="text-4xl md:text-6xl lg:text-7xl font-bold tracking-tight text-slate-900 dark:text-white mb-6 animate-fade-in-up-delay">
              {t("Manage Your School")}
            </h1>
              <span className="block text-3xl bg-gradient-to-r from-indigo-500 via-purple-500 to-violet-500 bg-clip-text text-transparent animate-fade-in-up-delay">
                {t("Smarter, Faster, Better")}
              </span>

            <p className="mx-auto max-w-2xl text-lg md:text-xl text-slate-600 dark:text-slate-300 mb-10 animate-fade-in-up-delay-2 leading-relaxed">
              {t("A comprehensive, modern school management platform that streamlines administration, enhances communication, and empowers educators to focus on what matters most - student success.")}
            </p>

            <div className="flex flex-col sm:flex-row items-center justify-center gap-4 animate-fade-in-up-delay-2">
              <button
                onClick={() => navigate("/register-school")}
                className="group inline-flex items-center gap-2 rounded-xl bg-gradient-to-r from-indigo-500 to-violet-500 px-8 py-4 text-base font-semibold text-white shadow-xl shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-105 transition-all duration-300"
              >
                {t("Get Started")}
                <ArrowRight className="h-5 w-5 group-hover:translate-x-1 transition-transform" />
              </button>
              <button
                onClick={() => {
                  const element = document.getElementById("features");
                  if (element) {
                    element.scrollIntoView({ behavior: "smooth" });
                  }
                }}
                className="group inline-flex items-center gap-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-white/80 dark:bg-slate-900/80 backdrop-blur-sm px-8 py-4 text-base font-semibold text-slate-900 dark:text-white hover:bg-slate-100 dark:hover:bg-slate-800 hover:scale-105 transition-all duration-300"
              >
                <Zap className="h-5 w-5 text-indigo-500" />
                {t("Explore Features")}
              </button>
            </div>

            {/* Stats */}
            <div className="mt-16 grid grid-cols-2 gap-8 md:grid-cols-4 animate-fade-in-up-delay-2">
              {stats.map((stat, index) => (
                <div key={index} className="text-center">
                  <div className="text-3xl md:text-4xl font-bold bg-gradient-to-r from-indigo-500 to-violet-500 bg-clip-text text-transparent mb-2">
                    {stat.value}
                  </div>
                  <div className="text-sm md:text-base text-slate-600 dark:text-slate-400 font-medium">
                    {stat.label}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Decorative image/illustration placeholder */}
        <div className="mt-16 mx-auto max-w-6xl px-4 sm:px-6 lg:px-8 animate-fade-in-up-delay-2">
          <div className="relative rounded-2xl overflow-hidden shadow-2xl shadow-indigo-500/20 border border-slate-200 dark:border-slate-700">
            <div className="aspect-[16/9] w-full bg-gradient-to-br from-slate-100 to-slate-200 dark:from-slate-800 dark:to-slate-900">
              <div className="absolute inset-0 flex items-center justify-center">
                <div className="text-center">
                  <Logo className="mx-auto mb-4 h-20 w-20 shadow-xl" />
                  <p className="text-lg font-semibold text-slate-700 dark:text-slate-300">
                    {t("Modern School Dashboard")}
                  </p>
                  <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                    {t("Intuitive • Powerful • Beautiful")}
                  </p>
                </div>
              </div>
              {/* Floating elements */}
              <div className="absolute top-8 left-8 flex items-center gap-2 rounded-lg bg-white/90 white:bg-slate-800/90 backdrop-blur-sm px-4 py-2 shadow-lg animate-float">
                <CheckCircle className="h-5 w-5 text-emerald-500" />
                <span className="text-sm font-medium">{t("Attendance Marked")}</span>
              </div>
              <div className="absolute top-8 right-8 flex items-center gap-2 rounded-lg bg-white/90 white:bg-slate-800/90 backdrop-blur-sm px-4 py-2 shadow-lg animate-float" style={{ animationDelay: "2s" }}>
                <Star className="h-5 w-5 text-yellow-500" />
                <span className="text-sm font-medium">{t("Top Performer")}</span>
              </div>
              <div className="absolute bottom-8 left-1/2 -translate-x-1/2 flex items-center gap-2 rounded-lg bg-white/90 white:bg-slate-800/90 backdrop-blur-sm px-4 py-2 shadow-lg animate-float" style={{ animationDelay: "4s" }}>
                <BarChart3 className="h-5 w-5 text-indigo-500" />
                <span className="text-sm font-medium">{t("Live Analytics")}</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="py-20 md:py-28 bg-gradient-to-b from-white to-slate-50 dark:from-slate-950 dark:to-slate-900">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <div className="inline-flex items-center gap-2 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-4 py-1.5 mb-6">
              <Sparkles className="h-4 w-4 text-indigo-400" />
              <span className="text-sm font-medium text-indigo-300">{t("Powerful Features")}</span>
            </div>
            <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-4">
              {t("Everything You Need to")}
              <span className="block bg-gradient-to-r from-indigo-500 to-violet-500 bg-clip-text text-transparent">
                {t("Run Your School Efficiently")}
              </span>
            </h2>
            <p className="mx-auto max-w-2xl text-lg text-slate-600 dark:text-slate-300">
              {t("From academics to administration, our all-in-one platform covers every aspect of school management.")}
            </p>
          </div>

          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
            {features.map((feature, index) => (
              <div
                key={index}
                className="group relative rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-6 shadow-sm hover:shadow-xl hover:shadow-indigo-500/10 transition-all duration-300 hover:-translate-y-1"
              >
                <div className="absolute inset-0 rounded-2xl bg-gradient-to-br opacity-0 group-hover:opacity-5 transition-opacity duration-300" style={{ backgroundImage: `linear-gradient(to br, var(--tw-gradient-from), var(--tw-gradient-to))` }}></div>
                <div className={`flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br ${feature.color} shadow-lg mb-4 group-hover:scale-110 transition-transform duration-300`}>
                  <feature.icon className="h-6 w-6 text-white" />
                </div>
                <h3 className="text-xl font-semibold text-slate-900 dark:text-white mb-2">
                  {feature.title}
                </h3>
                <p className="text-slate-600 dark:text-slate-300 leading-relaxed">
                  {feature.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* About Section */}
      <section id="about" className="py-20 md:py-28">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 gap-12 lg:grid-cols-2 lg:gap-16 items-center">
            <div className="animate-fade-in-up">
              <div className="inline-flex items-center gap-2 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-4 py-1.5 mb-6">
                <Globe className="h-4 w-4 text-indigo-400" />
                <span className="text-sm font-medium text-indigo-300">{t("Why Choose Us")}</span>
              </div>
              <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-6">
                {t("Built for Modern Educational")}
                <span className="block bg-gradient-to-r from-indigo-500 to-violet-500 bg-clip-text text-transparent">
                  {t("Institutions")}
                </span>
              </h2>
              <p className="text-lg text-slate-600 dark:text-slate-300 mb-6 leading-relaxed">
                {t("CampusERP is designed with educators, administrators, students, and parents in mind. Our intuitive interface and powerful features make school management effortless, allowing you to focus on creating an exceptional learning environment.")}
              </p>
              <p className="text-lg text-slate-600 dark:text-slate-300 mb-8 leading-relaxed">
                {t("With role-based portals for different stakeholders, real-time updates, and comprehensive reporting, we bridge the gap between home and school to foster better collaboration.")}
              </p>

              <div className="space-y-4 mb-8">
                {[
                  t("Intuitive and user-friendly interface"),
                  t("Role-based access for all stakeholders"),
                  t("Mobile-responsive design for access anywhere"),
                  t("Comprehensive data management and security"),
                  t("24/7 reliable performance and support"),
                ].map((item, index) => (
                  <div key={index} className="flex items-start gap-3">
                    <CheckCircle className="h-6 w-6 text-emerald-500 flex-shrink-0 mt-0.5" />
                    <span className="text-slate-700 dark:text-slate-200">{item}</span>
                  </div>
                ))}
              </div>

              <button
                onClick={() => navigate("/register-school")}
                className="group inline-flex items-center gap-2 rounded-xl bg-gradient-to-r from-indigo-500 to-violet-500 px-8 py-4 text-base font-semibold text-white shadow-xl shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-105 transition-all duration-300"
              >
                {t("Start Free Today")}
                <ArrowRight className="h-5 w-5 group-hover:translate-x-1 transition-transform" />
              </button>
            </div>

            <div className="relative animate-slide-in-right">
              <div className="absolute inset-0 bg-gradient-to-br from-indigo-500/20 to-violet-500/20 rounded-2xl blur-3xl"></div>
              <div className="relative rounded-2xl overflow-hidden shadow-2xl border border-slate-200 dark:border-slate-700">
                <div className="aspect-square bg-gradient-to-br from-slate-100 to-slate-200 dark:from-slate-800 dark:to-slate-900 p-8">
                  <div className="grid grid-cols-2 gap-4 h-full">
                    <div className="bg-white dark:bg-slate-800 rounded-xl p-4 shadow-lg flex flex-col items-center justify-center gap-2">
                      <Users className="h-10 w-10 text-indigo-500" />
                      <span className="text-sm font-medium text-slate-700 dark:text-slate-200">{t("Students")}</span>
                    </div>
                    <div className="bg-white dark:bg-slate-800 rounded-xl p-4 shadow-lg flex flex-col items-center justify-center gap-2">
                      <GraduationCap className="h-10 w-10 text-violet-500" />
                      <span className="text-sm font-medium text-slate-700 dark:text-slate-200">{t("Teachers")}</span>
                    </div>
                    <div className="bg-white dark:bg-slate-800 rounded-xl p-4 shadow-lg flex flex-col items-center justify-center gap-2 col-span-2">
                      <BookOpen className="h-12 w-12 text-emerald-500" />
                      <span className="text-sm font-medium text-slate-700 dark:text-slate-200">{t("Academics")}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Pricing Section */}
      <section id="pricing" className="py-20 md:py-28">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <div className="inline-flex items-center gap-2 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-4 py-1.5 mb-6">
              <Zap className="h-4 w-4 text-indigo-400" />
              <span className="text-sm font-medium text-indigo-300">{t("Simple Pricing")}</span>
            </div>
            <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-4">
              {t("Choose the Plan That")}
              <span className="block bg-gradient-to-r from-indigo-500 to-violet-500 bg-clip-text text-transparent">
                {t("Fits Your School")}
              </span>
            </h2>
            <p className="mx-auto max-w-2xl text-lg text-slate-600 dark:text-slate-300">
              {t("Start with the plan that matches your size today and upgrade as you grow. Payment is arranged directly with our team.")}
            </p>
          </div>

          {plansLoading ? (
            <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
              {[0, 1, 2].map((index) => (
                <div
                  key={index}
                  className="h-[420px] animate-pulse rounded-2xl border border-slate-200 bg-slate-100 dark:border-slate-700 dark:bg-slate-900"
                />
              ))}
            </div>
          ) : plans.length === 0 ? (
            <p className="text-center text-slate-500 dark:text-slate-400">
              {t("Pricing plans are being updated. Please contact us for details.")}
            </p>
          ) : (
            <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
              {plans.map((plan, index) => {
                const highlighted = plans.length > 1 && index === Math.floor(plans.length / 2);
                return (
                  <div
                    key={plan.id}
                    className={
                      highlighted
                        ? "relative rounded-2xl border-2 border-indigo-500 bg-white p-8 shadow-2xl shadow-indigo-500/20 dark:bg-slate-900 md:-translate-y-2"
                        : "relative rounded-2xl border border-slate-200 bg-white p-8 shadow-sm dark:border-slate-700 dark:bg-slate-900"
                    }
                  >
                    {highlighted && (
                      <span className="absolute -top-3 start-1/2 -translate-x-1/2 rounded-full bg-gradient-to-r from-indigo-500 to-violet-500 px-4 py-1 text-xs font-semibold text-white">
                        {t("Most popular")}
                      </span>
                    )}
                    <h3 className="text-xl font-semibold text-slate-900 dark:text-white">
                      {plan.name}
                    </h3>
                    <p className="mt-2 min-h-[48px] text-sm text-slate-600 dark:text-slate-300">
                      {localizedPlanDescription(plan) || t("Everything you need to run your school.")}
                    </p>
                    <div className="mt-6 flex items-end gap-1">
                      <span className="text-4xl font-bold text-slate-900 dark:text-white">
                        {plan.priceMonthly === 0
                          ? t("Free")
                          : `$${plan.priceMonthly.toLocaleString()}`}
                      </span>
                      {plan.priceMonthly > 0 && (
                        <span className="pb-1 text-sm text-slate-500 dark:text-slate-400">
                          / {t("month")}
                        </span>
                      )}
                    </div>

                    <ul className="mt-6 space-y-3">
                      {planHighlightLabels(plan).map((feature) => (
                        <li key={feature} className="flex items-start gap-2 text-sm text-slate-700 dark:text-slate-200">
                          <CheckCircle className="mt-0.5 h-4 w-4 shrink-0 text-emerald-500" />
                          <span>{feature}</span>
                        </li>
                      ))}
                    </ul>

                    <button
                      onClick={() => navigate(`/register-school?planId=${plan.id}`)}
                      className={
                        highlighted
                          ? "mt-8 w-full rounded-xl bg-gradient-to-r from-indigo-500 to-violet-500 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-indigo-500/25 transition hover:scale-[1.02]"
                          : "mt-8 w-full rounded-xl border border-slate-300 bg-white px-6 py-3 text-sm font-semibold text-slate-900 transition hover:border-indigo-500 hover:text-indigo-600 dark:border-slate-700 dark:bg-slate-900 dark:text-white dark:hover:text-indigo-400"
                      }
                    >
                      {t("Choose {plan}", { plan: plan.name })}
                    </button>
                  </div>
                );
              })}
            </div>
          )}

          <p className="mt-10 text-center text-sm text-slate-500 dark:text-slate-400">
            {t("Need a custom plan or want to pay by bank transfer?")}{" "}
            <a href="mailto:sales@campuserp.com" className="font-semibold text-indigo-600 hover:underline dark:text-indigo-400">
              {t("Contact us for payment")}
            </a>
          </p>
        </div>
      </section>

      {/* Testimonials Section */}
      <section className="py-20 md:py-28 bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-950">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <div className="inline-flex items-center gap-2 rounded-full border border-indigo-500/30 bg-indigo-500/10 px-4 py-1.5 mb-6">
              <Star className="h-4 w-4 text-indigo-400" />
              <span className="text-sm font-medium text-indigo-300">{t("Testimonials")}</span>
            </div>
            <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-4">
              {t("Loved by Educators")}
              <span className="block bg-gradient-to-r from-indigo-500 to-violet-500 bg-clip-text text-transparent">
                {t("Across the Globe")}
              </span>
            </h2>
            <p className="mx-auto max-w-2xl text-lg text-slate-600 dark:text-slate-300">
              {t("See what school administrators and teachers have to say about CampusERP.")}
            </p>
          </div>

          <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
            {testimonials.map((testimonial, index) => (
              <div
                key={index}
                className="rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-6 shadow-sm hover:shadow-xl hover:shadow-indigo-500/10 transition-all duration-300"
              >
                <div className="flex gap-1 mb-4">
                  {[...Array(testimonial.rating)].map((_, i) => (
                    <Star key={i} className="h-5 w-5 fill-yellow-400 text-yellow-400" />
                  ))}
                </div>
                <p className="text-slate-600 dark:text-slate-300 mb-6 leading-relaxed italic">
                  "{testimonial.content}"
                </p>
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-violet-500 text-white font-semibold">
                    {testimonial.name.split(" ").map(n => n[0]).join("")}
                  </div>
                  <div>
                    <h4 className="font-semibold text-slate-900 dark:text-white">{testimonial.name}</h4>
                    <p className="text-sm text-slate-500 dark:text-slate-400">{testimonial.role}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 md:py-28 relative">
        <div className="absolute inset-0 -z-10">
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-gradient-to-br from-indigo-500/25 to-violet-500/25 rounded-full blur-[120px]"></div>
        </div>
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-6">
            {t("Ready to Transform")}
            <span className="block bg-gradient-to-r from-indigo-500 to-violet-500 bg-clip-text text-transparent">
              {t("Your School Management?")}
            </span>
          </h2>
          <p className="text-lg md:text-xl text-slate-600 dark:text-slate-300 mb-10">
            {t("Join thousands of schools already using CampusERP to streamline their operations and drive success.")}
          </p>
          <button
            onClick={() => navigate("/register-school")}
            className="group inline-flex items-center gap-2 rounded-xl bg-gradient-to-r from-indigo-500 to-violet-500 px-10 py-5 text-lg font-semibold text-white shadow-2xl shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-105 transition-all duration-300"
          >
            {t("Get Started Now")}
            <ArrowRight className="h-6 w-6 group-hover:translate-x-1 transition-transform" />
          </button>
        </div>
      </section>
    </div>
  );
}