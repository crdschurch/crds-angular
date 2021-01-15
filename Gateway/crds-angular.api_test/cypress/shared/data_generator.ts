import { v4 as uuid } from 'uuid';
import { generate } from 'generate-password';

export function getUUID(): string {
  return uuid();
}

export function getTestPassword(): string {
  const pw = generate({
    length: 20,
    numbers: true,
    symbols: true,
    exclude: '"`'
  });
  return pw;
}

/**
 * Generates a unique email address
 * Warning! Accounts with these emails are intended to be temporary test accounts
 *   and are regularly deleted by a scheduled job.
 */
export function getTempTesterEmail(): string {
  return `mpcrds+auto+temp+${getUUID()}@gmail.com`;
}