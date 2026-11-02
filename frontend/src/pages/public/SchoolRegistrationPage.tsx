import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Form } from "antd";
import {
  Building2,
  Hash,
  MapPin,
  Phone,
  Mail,
  User,
  Lock,
  ArrowLeft,
  CheckCircle,
  CreditCard,
  Sparkles,
} from "lucide-react";
import { toast } from "sonner";
import Button from "@/components/atoms/Button";
import { Input, InputPassword } from "@/components/atoms/Input";
import api from "@/lib/axios";
import { t } from "@/i18n";
import { translateApiMessage } from "@/lib/errors";
import { usePublicPlans } from "@/features/billing/hooks";
import {
  localizedPlanDescription,
  planHighlightLabels,
} from "@/features/billing/display";

const schema = z.object({
  schoolName: z.string().min(1, t("School name is required")).max(200),
  subdomainCode: z
    .string()
    .min(1, t("Subdomain code is required"))
    .max(100)
    .regex(/^[a-z0-9-]+$/, t("Lowercase letters, numbers, and hyphens only")),
  address: z.string().max(500).optional().or(z.literal("")),
  phone: z.string().max(50).optional().or(z.literal("")),
  schoolEmail: z
    .string()
    .email(t("Invalid school email address"))
    .max(256)
    .optional()
    .or(z.literal("")),
  adminFirstName: z.string().min(1, t("Admin first name is required")).max(100),
  adminLastName: z.string().min(1, t("Admin last name is required")).max(100),
  adminEmail: z
    .string()
    .min(1, t("Admin email is required"))
    .email(t("Invalid admin email address"))
    .max(256),
  adminPassword: z
    .string()
    .min(8, t("Password must be at least 8 characters"))
    .regex(/[A-Z]/, t("Password must contain an uppercase letter"))
    .regex(/[a-z]/, t("Password must contain a lowercase letter"))
    .regex(/[0-9]/, t("Password must contain a digit"))
    .regex(/[^a-zA-Z0-9]/, t("Password must contain a non-alphanumeric character")),
});

type FormData = z.infer<typeof schema>;

export default function SchoolRegistrationPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const { data: plans = [], isLoading: plansLoading } = usePublicPlans();
  const [selectedPlanId, setSelectedPlanId] = useState<string | undefined>();

  useEffect(() => {
    if (plans.length === 0) return;
    const requested = searchParams.get("planId");
    if (selectedPlanId && plans.some((plan) => plan.id === selectedPlanId)) return;
    const match = plans.find((plan) => plan.id === requested);
    setSelectedPlanId(match?.id ?? plans[0].id);
  }, [plans, searchParams, selectedPlanId]);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      address: "",
      phone: "",
      schoolEmail: "",
    },
  });

  const onSubmit = async (data: FormData) => {
    if (!selectedPlanId) {
      toast.error(t("Please choose a subscription plan."));
      return;
    }
    setLoading(true);
    try {
      const payload = {
        schoolName: data.schoolName,
        subdomainCode: data.subdomainCode,
        address: data.address || undefined,
        phone: data.phone || undefined,
        schoolEmail: data.schoolEmail || undefined,
        adminFirstName: data.adminFirstName,
        adminLastName: data.adminLastName,
        adminEmail: data.adminEmail,
        adminPassword: data.adminPassword,
        subscriptionPlanId: selectedPlanId,
      };

      await api.post("/schoolregistrationrequests", payload);
      setSubmitted(true);
      toast.success(t("School registration request submitted successfully"));
    } catch (err: unknown) {
      const message =
        err && typeof err === "object" && "response" in err
          ? ((err as { response?: { data?: { error?: string; message?: string } } })
              .response?.data?.error ||
             (err as { response?: { data?: { message?: string } } })
               .response?.data?.message)
          : err instanceof Error
            ? err.message
            : t("Failed to submit registration request.");
      toast.error(translateApiMessage(message ?? t("Failed to submit registration request.")));
    } finally {
      setLoading(false);
    }
  };

  if (submitted) {
    return (
      <div className="mx-auto max-w-3xl px-4 pt-24 pb-16 sm:px-6 lg:px-8">
        <div className="rounded-2xl border border-emerald-500/20 bg-emerald-500/10 p-8 text-center shadow-lg backdrop-blur-sm">
          <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-emerald-500 shadow-lg">
            <CheckCircle className="h-8 w-8 text-white" />
          </div>
          <h1 className="mb-4 text-3xl font-bold text-slate-900 dark:text-white">
            {t("Request Submitted Successfully")}
          </h1>
          <p className="mb-6 text-lg text-slate-700 dark:text-slate-300">
            {t("Thank you for your interest in SchoolManager! Your school registration request has been submitted and is now pending review by our platform administrators.")}
          </p>
          <p className="mb-8 text-slate-600 dark:text-slate-400">
            {t("You'll be contacted once your request is reviewed and approved.")}
          </p>
          <div className="flex flex-col justify-center gap-4 sm:flex-row">
            <Button
              variant="primary"
              onClick={() => navigate("/")}
              className="px-8"
            >
              {t("Back to Home")}
            </Button>
            <Button
              variant="ghost"
              onClick={() => setSubmitted(false)}
              className="px-8"
            >
              {t("Submit Another Request")}
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-5xl px-4 pt-24 pb-16 sm:px-6 lg:px-8">
      <div className="mb-8">
        <button
          onClick={() => navigate(-1)}
          className="group mb-6 inline-flex items-center gap-2 text-sm font-medium text-slate-600 transition-colors hover:text-indigo-600 dark:text-slate-400 dark:hover:text-indigo-400"
        >
          <ArrowLeft className="h-4 w-4 transition-transform group-hover:-translate-x-1" />
          {t("Back")}
        </button>
        <h1 className="mb-2 text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl dark:text-white">
          {t("Register Your School")}
        </h1>
        <p className="text-lg text-slate-600 dark:text-slate-300">
          {t("Tell us about your school and create your administrator account. Our platform team will review your request and get you set up.")}
        </p>
      </div>

      <Form
        layout="vertical"
        onFinish={handleSubmit(onSubmit)}
        requiredMark={false}
        className="space-y-8"
      >
        {/* School Information */}
        <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm dark:border-slate-700 dark:bg-slate-900">
          <h2 className="mb-6 flex items-center gap-2 text-xl font-semibold text-slate-900 dark:text-white">
            <Building2 className="h-5 w-5 text-indigo-500" />
            {t("School Information")}
          </h2>

          <div className="grid gap-6 md:grid-cols-2">
            <Form.Item
              label={t("School Name")}
              validateStatus={errors.schoolName ? "error" : ""}
              help={errors.schoolName?.message}
              required
            >
              <Controller
                name="schoolName"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    placeholder={t("e.g. Greenfield Academy")}
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t("Subdomain Code")}
              validateStatus={errors.subdomainCode ? "error" : ""}
              help={errors.subdomainCode?.message}
              required
            >
              <Controller
                name="subdomainCode"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    prefix={<Hash className="h-4 w-4 text-slate-400" />}
                    placeholder={t("e.g. greenfield")}
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t("School Email")}
              validateStatus={errors.schoolEmail ? "error" : ""}
              help={errors.schoolEmail?.message}
              className="md:col-span-2"
            >
              <Controller
                name="schoolEmail"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    prefix={<Mail className="h-4 w-4 text-slate-400" />}
                    placeholder={t("info@school.com")}
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t("Phone Number")}
              validateStatus={errors.phone ? "error" : ""}
              help={errors.phone?.message}
            >
              <Controller
                name="phone"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    prefix={<Phone className="h-4 w-4 text-slate-400" />}
                    placeholder="+1 (555) 123-4567"
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t("Address")}
              validateStatus={errors.address ? "error" : ""}
              help={errors.address?.message}
              className="md:col-span-2"
            >
              <Controller
                name="address"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    prefix={<MapPin className="h-4 w-4 text-slate-400" />}
                    placeholder={t("Full school address")}
                    size="large"
                  />
                )}
              />
            </Form.Item>
          </div>
        </div>

        {/* School Admin Information */}
        <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm dark:border-slate-700 dark:bg-slate-900">
          <h2 className="mb-6 flex items-center gap-2 text-xl font-semibold text-slate-900 dark:text-white">
            <User className="h-5 w-5 text-indigo-500" />
            {t("School Administrator Details")}
          </h2>

          <div className="grid gap-6 md:grid-cols-2">
            <Form.Item
              label={t("First Name")}
              validateStatus={errors.adminFirstName ? "error" : ""}
              help={errors.adminFirstName?.message}
              required
            >
              <Controller
                name="adminFirstName"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    prefix={<User className="h-4 w-4 text-slate-400" />}
                    placeholder={t("First name")}
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t("Last Name")}
              validateStatus={errors.adminLastName ? "error" : ""}
              help={errors.adminLastName?.message}
              required
            >
              <Controller
                name="adminLastName"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    placeholder={t("Last name")}
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t("Admin Email")}
              validateStatus={errors.adminEmail ? "error" : ""}
              help={errors.adminEmail?.message}
              required
              className="md:col-span-2"
            >
              <Controller
                name="adminEmail"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    prefix={<Mail className="h-4 w-4 text-slate-400" />}
                    placeholder={t("admin@school.com")}
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t("Admin Password")}
              validateStatus={errors.adminPassword ? "error" : ""}
              help={errors.adminPassword?.message}
              required
              className="md:col-span-2"
            >
              <Controller
                name="adminPassword"
                control={control}
                render={({ field }) => (
                  <InputPassword
                    {...field}
                    prefix={<Lock className="h-4 w-4 text-slate-400" />}
                    placeholder={t("Enter a secure password")}
                    size="large"
                  />
                )}
              />
              <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">
                {t("Must be at least 8 characters with uppercase, lowercase, digit, and non-alphanumeric character.")}
              </p>
            </Form.Item>
          </div>
        </div>

        {/* Subscription Plan */}
        <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm dark:border-slate-700 dark:bg-slate-900">
          <h2 className="mb-2 flex items-center gap-2 text-xl font-semibold text-slate-900 dark:text-white">
            <CreditCard className="h-5 w-5 text-indigo-500" />
            {t("Choose a Subscription Plan")}
          </h2>
          <p className="mb-6 text-sm text-slate-600 dark:text-slate-300">
            {t("You can change your plan later. After approval, our team will send the first invoice and arrange payment with you.")}
          </p>

          {plansLoading ? (
            <div className="grid gap-4 md:grid-cols-3">
              {[0, 1, 2].map((index) => (
                <div
                  key={index}
                  className="h-48 animate-pulse rounded-xl border border-slate-200 bg-slate-100 dark:border-slate-700 dark:bg-slate-800"
                />
              ))}
            </div>
          ) : plans.length === 0 ? (
            <div className="rounded-xl border border-amber-300 bg-amber-50 p-4 text-sm text-amber-800 dark:border-amber-500/40 dark:bg-amber-500/10 dark:text-amber-200">
              {t("No plans are available right now. Please contact us and we will set up your subscription manually.")}
            </div>
          ) : (
            <div className="grid gap-4 md:grid-cols-3">
              {plans.map((plan) => {
                const selected = selectedPlanId === plan.id;
                return (
                  <button
                    key={plan.id}
                    type="button"
                    onClick={() => setSelectedPlanId(plan.id)}
                    className={
                      selected
                        ? "relative rounded-xl border-2 border-indigo-500 bg-indigo-50/60 p-5 text-start shadow-md dark:bg-indigo-500/10"
                        : "relative rounded-xl border border-slate-200 bg-white p-5 text-start transition hover:border-indigo-300 dark:border-slate-700 dark:bg-slate-900"
                    }
                  >
                    {selected && (
                      <span className="absolute top-3 end-3 flex h-6 w-6 items-center justify-center rounded-full bg-indigo-500 text-white">
                        <CheckCircle className="h-4 w-4" />
                      </span>
                    )}
                    <h3 className="text-base font-semibold text-slate-900 dark:text-white">
                      {plan.name}
                    </h3>
                    <div className="mt-1 flex items-end gap-1">
                      <span className="text-2xl font-bold text-slate-900 dark:text-white">
                        {plan.priceMonthly === 0
                          ? t("Free")
                          : `$${plan.priceMonthly.toLocaleString()}`}
                      </span>
                      {plan.priceMonthly > 0 && (
                        <span className="pb-0.5 text-xs text-slate-500 dark:text-slate-400">
                          / {t("month")}
                        </span>
                      )}
                    </div>
                    <p className="mt-2 line-clamp-2 text-xs text-slate-600 dark:text-slate-300">
                      {localizedPlanDescription(plan) || t("Everything you need to run your school.")}
                    </p>
                    <ul className="mt-3 space-y-1.5">
                      {planHighlightLabels(plan).map((feature) => (
                        <li
                          key={feature}
                          className="flex items-start gap-1.5 text-xs text-slate-700 dark:text-slate-200"
                        >
                          <CheckCircle className="mt-0.5 h-3.5 w-3.5 shrink-0 text-emerald-500" />
                          <span>{feature}</span>
                        </li>
                      ))}
                    </ul>
                  </button>
                );
              })}
            </div>
          )}

          <div className="mt-6 flex items-start gap-2 rounded-xl border border-indigo-200 bg-indigo-50 p-4 text-sm text-indigo-900 dark:border-indigo-500/30 dark:bg-indigo-500/10 dark:text-indigo-200">
            <Sparkles className="mt-0.5 h-4 w-4 shrink-0" />
            <span>
              {t("Payment is arranged directly with our team. Your subscription period starts automatically once the invoice is marked as paid.")}{" "}
              <a href="mailto:sales@campuserp.com" className="font-semibold underline">
                {t("Contact us for payment")}
              </a>
            </span>
          </div>
        </div>

        <div className="flex flex-col justify-end gap-4 sm:flex-row">
          <Button
            variant="ghost"
            onClick={() => navigate("/")}
            size="large"
            className="px-8"
          >
            {t("Cancel")}
          </Button>
          <Button
            variant="primary"
            htmlType="submit"
            loading={loading}
            size="large"
            className="px-8"
          >
            {t("Submit Registration Request")}
          </Button>
        </div>
      </Form>
    </div>
  );
}
