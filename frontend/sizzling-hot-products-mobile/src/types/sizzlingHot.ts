export type SizzlingHotResultType = 'single' | 'range';

export type SizzlingHotResult = {
  type: SizzlingHotResultType;
  period: string;
  productName: string;
};
