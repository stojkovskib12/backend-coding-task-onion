import { AlertCircle, CheckCircle2, LoaderCircle } from 'lucide-react';

export function ApiFeedback({ busy, error, success }: { busy?: boolean; error?: string; success?: string }) {
  if (busy) return <div className="feedback muted"><LoaderCircle className="spin" size={16} /> Working…</div>;
  if (error) return <div role="alert" className="feedback error"><AlertCircle size={16} />{error}</div>;
  if (success) return <div role="status" className="feedback success"><CheckCircle2 size={16} />{success}</div>;
  return null;
}
