import { forwardRef, type ComponentProps, type ComponentRef } from "react";
import { Input as AntInput, type InputProps as AntInputProps, type InputRef } from "antd";
import { cn } from "@/lib/utils";

export interface InputProps extends AntInputProps {
  className?: string;
  children?: React.ReactNode;
}

export const Input = forwardRef<InputRef, InputProps>(({ className, ...props }, ref) => {
  return <AntInput ref={ref} className={cn(className)} {...props} />;
});

Input.displayName = "Input";

export const InputPassword = forwardRef<InputRef, InputProps>((props, ref) => {
  return <AntInput.Password ref={ref} {...props} />;
});

InputPassword.displayName = "InputPassword";

export type TextAreaProps = ComponentProps<typeof AntInput.TextArea>;

export const TextArea = forwardRef<
  ComponentRef<typeof AntInput.TextArea>,
  TextAreaProps
>(({ className, ...props }, ref) => (
  <AntInput.TextArea ref={ref} className={cn(className)} {...props} />
));

TextArea.displayName = "TextArea";

export default Input;
