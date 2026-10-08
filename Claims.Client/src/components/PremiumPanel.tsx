import { useState } from 'react';
import type { CoverType } from '../api/contracts';
import type { ClaimsApi } from '../api/ClaimsApi';
import { ApiFeedback } from './ApiFeedback';
import { Field, Input, Select } from './Field';
const types: CoverType[] = ['Yacht', 'PassengerShip', 'ContainerShip', 'BulkCarrier', 'Tanker'];
export function PremiumPanel({ api }: { api: ClaimsApi }) {
  const [type,setType]=useState<CoverType>('Yacht'), [start,setStart]=useState(new Date().toISOString().slice(0,10)), [end,setEnd]=useState(''), [value,setValue]=useState<number|null>(null), [error,setError]=useState(''), [busy,setBusy]=useState(false);
  const calculate=async(e:React.FormEvent)=>{e.preventDefault();setBusy(true);setError('');setValue(null);try{setValue(await api.computePremium(start,end,type));}catch(err){setError(err instanceof Error?err.message:'Request failed.');}finally{setBusy(false);}};
  return <section className="panel-content"><div className="section-heading"><div><div className="eyebrow">PRICING ENGINE</div><h2>Premium estimate</h2><p>Calculate a premium for a coverage period and object type.</p></div></div><div className="action-grid premium-grid"><form className="card form-card" onSubmit={calculate}><h3>Calculation inputs</h3><Field label="Object type"><Select value={type} onChange={e=>setType(e.target.value as CoverType)}>{types.map(t=><option key={t}>{t}</option>)}</Select></Field><div className="field-row"><Field label="Start date"><Input type="date" required value={start} onChange={e=>setStart(e.target.value)}/></Field><Field label="End date"><Input type="date" required min={start} value={end} onChange={e=>setEnd(e.target.value)}/></Field></div><button className="button primary" disabled={busy}>Calculate premium</button><ApiFeedback busy={busy} error={error}/></form><div className="card premium-result"><span className="eyebrow">ESTIMATED PREMIUM</span><strong>{value===null?'—':value.toLocaleString(undefined,{maximumFractionDigits:2})}</strong><p>Calculated by the backend premium rules.</p></div></div></section>;
}
