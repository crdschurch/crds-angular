import { v4 as uuid } from 'uuid';
import { generate } from 'generate-password';

export function getUUID(): string {
  return uuid();
}

export function getTestPassword(): string {
  const pw = generate({
    length: 20,
    numbers: true,
    symbols: true
  });
  return pw;
}

export function getTempTesterEmail(): string {
  return `mpcrds+auto+temp+${getUUID()}@gmail.com`;
}