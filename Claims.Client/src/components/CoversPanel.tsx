import { useEffect, useState } from 'react';
import { Plus, RefreshCw, Trash2 } from 'lucide-react';
import type { Cover, CoverInput, CoverType } from '../api/contracts';
import type { ClaimsApi } from '../api/ClaimsApi';
import { ApiFeedback } from './ApiFeedback';
import { Field, Input, Select } from './Field';

const types: CoverType[] = ['Yacht', 'PassengerShip', 'ContainerShip', 'BulkCarrier', 'Tanker'];
export function CoversPanel({ api, onChange }: { api: ClaimsApi; onChange: (covers: Cover[]) => void }) {
  const [items, setItems] = useState<Cover[]>([]), [busy, setBusy] = useState(false), [error, setError] = useState(''), [success, setSuccess] = useState('');
  const [form, setForm] = useState<CoverInput>({ type: 'Yacht', startDate: new Date().toISOString().slice(0, 10), endDate: '' });
  useEffect(() => { let active = true; api.listCovers().then(rows => { if (active) { setItems(rows); onChange(rows); } }).catch(e => { if (active) setError(e instanceof Error ? e.message : 'Could not load covers.'); }); return () => { active = false; }; }, [api, onChange]);
  const run = async (action: () => Promise<void>) => { setBusy(true); setError(''); setSuccess(''); try { await action(); } catch (e) { setError(e instanceof Error ? e.message : 'Request failed.'); } finally { setBusy(false); } };
  const refresh = () => run(async () => { const rows = await api.listCovers(); setItems(rows); onChange(rows); setSuccess('Covers refreshed.'); });
  const create = (e: React.FormEvent) => { e.preventDefault(); return run(async () => { await api.createCover(form); const rows = await api.listCovers(); setItems(rows); onChange(rows); setSuccess('Cover created.'); }); };
  return <section className="panel-content"><div className="section-heading"><div><div className="eyebrow">POLICY REGISTER</div><h2>Covers</h2><p>Manage insured objects and coverage periods.</p></div><button className="button secondary" onClick={refresh}><RefreshCw size={16}/> Refresh</button></div>
    <form className="card form-card compact-form" onSubmit={create}><h3><Plus size={17}/> New cover</h3><div className="field-row three"><Field label="Object type"><Select value={form.type} onChange={e => setForm({...form,type:e.target.value as CoverType})}>{types.map(t=><option key={t}>{t}</option>)}</Select></Field><Field label="Start date"><Input type="date" required min={new Date().toISOString().slice(0,10)} value={form.startDate} onChange={e=>setForm({...form,startDate:e.target.value})}/></Field><Field label="End date"><Input type="date" required min={form.startDate} value={form.endDate} onChange={e=>setForm({...form,endDate:e.target.value})}/></Field></div><button disabled={busy} className="button primary">Create cover</button></form>
    <ApiFeedback busy={busy} error={error} success={success}/><div className="cover-grid">{items.map(c=><article className="card cover-card" key={c.id}><div className="cover-icon">{c.type === 'Yacht' ? '⛵' : '◈'}</div><div className="cover-type">{c.type}</div><div className="cover-id">COVER #{c.displayId}</div><div className="cover-period">{c.startDate?.slice(0,10)} <span>→</span> {c.endDate?.slice(0,10)}</div><button aria-label={`Delete cover ${c.displayId}`} className="icon-button danger-text" onClick={()=>run(async()=>{await api.deleteCover(c.displayId); const rows=items.filter(x=>x.id!==c.id);setItems(rows);onChange(rows);setSuccess(`Cover #${c.displayId} deleted.`);})}><Trash2 size={16}/></button></article>)}{!items.length&&<div className="card empty">Refresh to load covers from the API.</div>}</div>
  </section>;
}
