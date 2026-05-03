import { API_BASE_URL, SIZZLING_HOT_RESULTS_ENDPOINT } from '@/src/config/apiConfig';
import type { SizzlingHotResult, SizzlingHotResultType } from '@/src/types/sizzlingHot';

type SizzlingHotApiResult = {
  type?: unknown;
  period?: unknown;
  productName?: unknown;
};

export async function fetchSizzlingHotResults(): Promise<SizzlingHotResult[]> {
  const response = await fetch(`${API_BASE_URL}${SIZZLING_HOT_RESULTS_ENDPOINT}`);

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`);
  }

  const data: unknown = await response.json();

  if (!Array.isArray(data)) {
    throw new Error('Unexpected response shape');
  }

  return data.map(normalizeSizzlingHotResult);
}

function normalizeSizzlingHotResult(item: unknown): SizzlingHotResult {
  if (!isSizzlingHotApiResult(item)) {
    throw new Error('Unexpected result item shape');
  }

  const { type, period, productName } = item;

  if (!isSizzlingHotResultType(type) || typeof period !== 'string' || typeof productName !== 'string') {
    throw new Error('Unexpected result item shape');
  }

  return {
    type,
    period,
    productName,
  };
}

function isSizzlingHotApiResult(item: unknown): item is SizzlingHotApiResult {
  return typeof item === 'object' && item !== null;
}

function isSizzlingHotResultType(type: unknown): type is SizzlingHotResultType {
  return type === 'single' || type === 'range';
}
