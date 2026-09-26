import type { ButtonHTMLAttributes } from 'react'

type Variant = 'primary' | 'secondary' | 'ghost'
type Size = 'sm' | 'md' | 'lg'

const variantClasses: Record<Variant, string> = {
  primary: 'bg-accent text-white hover:bg-accent-strong disabled:bg-faint',
  secondary: 'border border-line bg-surface text-ink hover:bg-surface-2 disabled:text-faint',
  ghost: 'text-accent hover:bg-accent-soft disabled:text-faint',
}

const sizeClasses: Record<Size, string> = {
  sm: 'h-8 px-3 text-[13px]',
  md: 'h-9 px-3.5 text-[13px]',
  lg: 'h-11 px-4 text-sm',
}

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  size?: Size
}

export function Button({ variant = 'primary', size = 'md', className = '', type = 'button', ...props }: ButtonProps) {
  return (
    <button
      type={type}
      className={`inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-lg font-semibold transition-colors disabled:cursor-not-allowed ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      {...props}
    />
  )
}
