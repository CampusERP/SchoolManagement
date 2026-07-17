import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Form, Input, Button } from "antd";
import { Mail, Lock } from "lucide-react";
import { toast } from "sonner";
import { useAuthStore } from "@/store/authStore";
import { AuthApi } from "@/features/auth/api";
import { ROLE_HOME } from "@/lib/constants";

const loginSchema = z.object({
  email: z.string().min(1, "Email is required").email("Invalid email address"),
  password: z.string().min(1, "Password is required"),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setLoading(true);
    try {
      const response = await AuthApi.login(data);
      setAuth(response);
      toast.success("Welcome back!");
      const role = useAuthStore.getState().user?.role;
      if (role) {
        navigate(ROLE_HOME[role]);
      } else {
        navigate("/auth/login");
      }
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Login failed. Please try again.";
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="rounded-2xl bg-[var(--color-surface-card)] p-8 shadow-[var(--shadow-elevated)]">
      <div className="mb-8 text-center">
        <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-[var(--color-primary-light)] text-[var(--color-primary)] font-bold text-lg md:hidden">
          SM
        </div>
        <h2 className="text-2xl font-semibold text-[var(--color-text-primary)]">
          Welcome back
        </h2>
        <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
          Sign in to your account
        </p>
      </div>

      <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
        <Form.Item
          label="Email"
          validateStatus={errors.email ? "error" : ""}
          help={errors.email?.message}
        >
          <Controller
            name="email"
            control={control}
            render={({ field }) => (
              <Input
                {...field}
                prefix={<Mail className="h-4 w-4 text-[var(--color-text-muted)]" />}
                placeholder="you@example.com"
                size="large"
              />
            )}
          />
        </Form.Item>

        <Form.Item
          label="Password"
          validateStatus={errors.password ? "error" : ""}
          help={errors.password?.message}
        >
          <Input.Password
            prefix={<Lock className="h-4 w-4 text-[var(--color-text-muted)]" />}
            placeholder="Enter your password"
            size="large"
            {...register("password")}
          />
        </Form.Item>

        <Form.Item>
          <Button
            type="primary"
            htmlType="submit"
            size="large"
            loading={loading}
            block
          >
            Sign In
          </Button>
        </Form.Item>
      </Form>
    </div>
  );
}
