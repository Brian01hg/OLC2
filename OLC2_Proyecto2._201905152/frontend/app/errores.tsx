'use client';
import { useEffect, useState } from 'react';

export default function ErroresPage() {
  const [html, setHtml] = useState<string>('');

  useEffect(() => {
    fetch('http://localhost:5120/errores')
      .then(res => res.text())
      .then(setHtml)
      .catch(() => setHtml('<h1>Error cargando el reporte de errores</h1>'));
  }, []);

  return (
    <div dangerouslySetInnerHTML={{ __html: html }} />
  );
}
