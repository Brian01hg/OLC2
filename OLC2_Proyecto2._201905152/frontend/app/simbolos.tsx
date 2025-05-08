'use client';
import { useEffect, useState } from 'react';

export default function SimbolosPage() {
  const [html, setHtml] = useState<string>('');

  useEffect(() => {
    fetch('http://localhost:5120/simbolos')
      .then(res => res.text())
      .then(setHtml)
      .catch(() => setHtml('<h1>Error cargando la tabla de símbolos</h1>'));
  }, []);

  return (
    <div dangerouslySetInnerHTML={{ __html: html }} />
  );
}
