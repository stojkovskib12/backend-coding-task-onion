import { useEffect, useState } from 'react';
import { Plus, Trash2, RefreshCw, Search } from 'lucide-react';
import type { Claim, ClaimInput, ClaimType, Cover } from '../api/contracts';
import type { ClaimsApi } from '../api/ClaimsApi';
import { ApiFeedback } from './ApiFeedback';
import { Field, Input, Select } from './Field';

export function ClaimsPanel({ api, covers }: { api: ClaimsApi; covers: Cover[] }) {
  const [items, setItems] = useState<Claim[]>([]), [busy, setBusy] = useState(false), [error, setError] = useState(''), [success, setSuccess] = useState('');
  const [id, setId] = useState(''), [result, setResult] = useState<Claim | null>(null);
  const [form, setForm] = useState<ClaimInput>({ coverId: '', created: new Date().toISOString().slice(0, 10), name: '', type: 'Collision', damageCost: 0 });
  useEffect(() => { let active = true; api.listClaims().then(rows => { if (active) setItems(rows); }).catch(e => { if (active) setError(e instanceof Error ? e.message : 'Could not load claims.'); }); return () => { active = false; }; }, [api]);
  const run = async (action: () => Promise<void>) => { setBusy(true); setError(''); setSuccess(''); try { await action(); } catch (e) { setError(e instanceof Error ? e.message : 'Request failed.'); } finally { setBusy(false); } };
  const load = () => run(async () => { setItems(await api.listClaims()); setSuccess('Claims refreshed.'); });
  const create = (e: React.FormEvent) => { e.preventDefault(); return run(async () => { await api.createClaim(form); setSuccess('Claim created.'); setForm(v => ({ ...v, name: '', damageCost: 0 })); setItems(await api.listClaims()); }); };
  return <section className="panel-content">
    <div className="section-heading"><div><div className="eyebrow">CLAIM REGISTER</div><h2>Claims</h2><p>Create and manage reported losses.</p></div><button className="button secondary" onClick={load}><RefreshCw size={16}/> Refresh</button></div>
    <div className="action-grid"><form className="card form-card" onSubmit={create}><h3><Plus size={17}/> New claim</h3>
      <Field label="Related cover"><Select required value={form.coverId} onChange={e => setForm({ ...form, coverId: e.target.value })}><option value="" disabled>Select a cover</option>{covers.map(c => <option value={c.id} key={c.id}>Cover #{c.displayId} · {c.type}</option>)}</Select></Field>
      <Field label="Claim name"><Input required maxLength={200} placeholder="e.g. Hull damage claim" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })}/></Field>
      <div className="field-row"><Field label="Claim type"><Select value={form.type} onChange={e => setForm({ ...form, type: e.target.value as ClaimType })}>{(['Collision', 'Grounding', 'BadWeather', 'Fire'] as const).map(type => <option key={type}>{type}</option>)}</Select></Field><Field label="Created date"><Input required type="date" value={form.created} onChange={e => setForm({ ...form, created: e.target.value })}/></Field></div>
      <Field label="Damage cost"><Input required type="number" min="0" max="100000" step="0.01" value={form.damageCost} onChange={e => setForm({ ...form, damageCost: Number(e.target.value) })}/></Field><button disabled={busy || !covers.length} className="button primary">Create claim</button>{!covers.length && <small>Create a cover first.</small>}
    </form><div className="card form-card"><h3><Search size={17}/> Find claim</h3><Field label="Claim ID"><Input type="number" min="1" value={id} onChange={e => setId(e.target.value)} placeholder="e.g. 12"/></Field><button disabled={busy || !id || Number(id) < 1} className="button secondary" onClick={() => run(async () => setResult(await api.getClaim(Number(id))))}>Get claim</button>{result && <pre className="result-json">{JSON.stringify(result, null, 2)}</pre>}</div></div>
    <ApiFeedback busy={busy} error={error} success={success}/><div className="card table-card"><div className="table-title"><h3>All claims</h3><span>{items.length} records</span></div>{items.length ? <div className="table-wrap"><table><thead><tr><th>ID</th><th>Name</th><th>Cover</th><th>Type</th><th>Created</th><th>Damage cost</th><th/></tr></thead><tbody>{items.map(c => { const cover = covers.find(item => item.id === c.coverId); return <tr key={c.id}><td>#{c.displayId}</td><td>{c.name}</td><td>{cover ? `${cover.type} · Cover #${cover.displayId}` : 'Cover unavailable'}</td><td>{c.type}</td><td>{c.created?.slice(0,10)}</td><td>{c.damageCost.toLocaleString(undefined,{maximumFractionDigits:2})}</td><td><button type="button" aria-label={`Delete claim ${c.displayId}`} className="icon-button danger-text" onClick={() => run(async () => { await api.deleteClaim(c.displayId); setItems(items.filter(x => x.id !== c.id)); setSuccess(`Claim #${c.displayId} deleted.`); })}><Trash2 size={16}/></button></td></tr>;})}</tbody></table></div> : <div className="empty">No claims found. Create one or refresh the list.</div>}</div>
  </section>;
}
